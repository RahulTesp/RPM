using Dapper;
using Microsoft.Data.SqlClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;

public class PatientSmsQueueProcessor : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly string _conn;
    private readonly int _interval;

    public PatientSmsQueueProcessor(IConfiguration config)
    {
        _config = config;
        _conn = config.GetConnectionString("DefaultConnection");
        _interval = config.GetValue<int>("PollingIntervalSeconds");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessQueueAsync();
            await Task.Delay(_interval * 1000, stoppingToken);
        }
    }

    private async Task ProcessQueueAsync()
    {
        using var con = new SqlConnection(_conn);

        var pending = await con.QueryAsync<PatientSmsQueueItem>(
            "SELECT TOP 10 * FROM PatientSMSQueue WHERE Processed = 0 ORDER BY QueueId ASC");

        foreach (var item in pending)
        {
            await ProcessSmsAsync(item, con);
        }
    }

    private async Task ProcessSmsAsync(PatientSmsQueueItem item, SqlConnection con)
    {
        try
        {
            var twilioConfig = GetTwilioConfigFromDb(_conn);
            if (twilioConfig == null)
            {
                // Log missing Twilio configuration
                Console.WriteLine("[ERROR] Twilio configuration is missing.");
                return;
            }

            string accSid = twilioConfig.AccountSid;
            string authToken = twilioConfig.AuthToken;
            string serviceSid = twilioConfig.MessagingServiceSid;
            string fromPhone = twilioConfig.FromPhoneNumber;
            string CountryCode = twilioConfig.CountryCode;

            // Initialize Twilio
            TwilioClient.Init(accSid, authToken);

            var fullPhoneNumber = $"{CountryCode}{item.MobileNo}";
            var msgOptions = new CreateMessageOptions(new PhoneNumber(fullPhoneNumber))
            {
                MessagingServiceSid = serviceSid,
                Body = $"Dear Patient {item.PatientNumber}, your account is now inactive. Please contact support if you have questions."
            };
            var message = await MessageResource.CreateAsync(msgOptions);

            // Log SMS details to console
            Console.WriteLine(
                $"[SMS SENT] Patient: {item.PatientNumber}, " +
                $"To: {fullPhoneNumber}, " +
                $"CountryCode: {CountryCode}, " +
                $"MessageSid: {message.Sid}, " +
                $"Status: {message.Status}, " +
                $"Body: {msgOptions.Body}"
            );
            // Mark queue record as processed
            await con.ExecuteAsync(
                "UPDATE PatientSMSQueue SET Processed = 1 WHERE QueueId = @QId",
                new { QId = item.QueueId });

            // Log SMS details
            await LogSmsToDb(item, message, con, fromPhone);


        }
        catch (Exception ex)
        {
            // Log SMS details to console
            Console.WriteLine($"[ERROR] Failed to process SMS for Patient: {item.PatientNumber}. Error: {ex.Message}");

        }
    }

    private async Task<int> LogSmsToDb(PatientSmsQueueItem item, MessageResource msg, SqlConnection con, string fromPhone)
    {
        try
        {
            using (var command = new SqlCommand("usp_InsPatientSMSDetails", con))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CreatedBy", "WebJob");
                command.Parameters.AddWithValue("@PatientUserName", item.PatientNumber);
                command.Parameters.AddWithValue("@fromNo", fromPhone);
                command.Parameters.AddWithValue("@toNo", item.MobileNo);
                command.Parameters.AddWithValue("@Body", msg.Body);
                command.Parameters.AddWithValue("@SentDate", msg.DateCreated);
                command.Parameters.AddWithValue("@Direction", msg.Direction.ToString());
                command.Parameters.AddWithValue("@Status", msg.Status.ToString());

                var returnParameter = command.Parameters.Add("RetVal", System.Data.SqlDbType.Int);
                returnParameter.Direction = System.Data.ParameterDirection.ReturnValue;

                if (con.State != System.Data.ConnectionState.Open)
                    await con.OpenAsync();

                await command.ExecuteReaderAsync();
                int retVal = (int)(returnParameter.Value ?? 0);

                con.Close();
                return retVal;
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    private static TwilioConfig? GetTwilioConfigFromDb(string connectionString)
    {
        try
        {
            using var con = new SqlConnection(connectionString);

            // Read Twilio config
            var twilioCmd = new SqlCommand("usp_GetSystemConfig", con)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            twilioCmd.Parameters.Add("@Category", System.Data.SqlDbType.NVarChar).Value = "Twilio";
            twilioCmd.Parameters.Add("@CreatedBy", System.Data.SqlDbType.NVarChar).Value = "User";

            con.Open();
            var configList = new List<SystemConfigInfo>();
            using (var reader = twilioCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var info = new SystemConfigInfo
                    {
                        Name = reader["name"]?.ToString() ?? string.Empty,
                        Value = reader["Value"]?.ToString() ?? string.Empty,
                        Descripiton = reader["Description"]?.ToString() ?? string.Empty
                    };
                    configList.Add(info);
                }
            }

            var accSid = configList.Find(x => x.Name.Equals("AccountSID"));
            var authToken = configList.Find(x => x.Name.Equals("AuthToken"));
            var smsSid = configList.Find(x => x.Name.Equals("SMSServiceSID"));
            var fromPhone = configList.Find(x => x.Name.Equals("MyPhoneNumber"));
            var systemPhone = configList.Find(x => x.Name.Equals("MyPhoneNumber")); // Same as above, but explicit for clarity

            // Read CountryCode from Client category
            var countryCmd = new SqlCommand("usp_GetSystemConfig", con)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            countryCmd.Parameters.Add("@Category", System.Data.SqlDbType.NVarChar).Value = "Client";
            countryCmd.Parameters.Add("@CreatedBy", System.Data.SqlDbType.NVarChar).Value = "CountryCode";

            string countryCode = string.Empty;
            using (var reader = countryCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    countryCode = reader["Value"]?.ToString() ?? string.Empty;
                }
            }

            if (accSid == null || authToken == null || smsSid == null || fromPhone == null || systemPhone == null)
                return null;

            return new TwilioConfig
            {
                AccountSid = accSid.Value,
                AuthToken = authToken.Value,
                MessagingServiceSid = smsSid.Value,
                FromPhoneNumber = fromPhone.Value,
                CountryCode = countryCode,
                SystemPhoneNumber = systemPhone.Value
            };
        }
        catch (Exception ex)
        {
            // Optional: log exception
            return null;
        }

    }
    public class PatientSmsQueueItem
    {
        public int QueueId { get; set; }
        public int PatientId { get; set; }
        public string PatientNumber { get; set; }
        public string MobileNo { get; set; }
    }
    public class TwilioConfig
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string MessagingServiceSid { get; set; }
        public string FromPhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string SystemPhoneNumber { get; set; }
    }

    public class SystemConfigInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descripiton { get; set; }

    }
}