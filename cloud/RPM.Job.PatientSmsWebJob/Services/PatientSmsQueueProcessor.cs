using Dapper;
using Microsoft.Data.SqlClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;

public class PatientSmsQueueProcessor
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

    public async Task RunOnceAsync()
    {
        await ProcessQueueAsync();
    }

    private async Task ProcessQueueAsync()
    {
        using var con = new SqlConnection(_conn);
        await con.OpenAsync();

        // Load all enabled SMS rules
        var rules = await con.QueryAsync<SmsRule>(
            "SELECT Id, RuleName, SelectClause, FromTable, JoinClause, ConditionClause, TemplateMessage FROM SMSRules WHERE IsEnabled = 1");

        foreach (var rule in rules)
        {
            var query = $@"
                SELECT {rule.SelectClause}
                FROM {rule.FromTable}
                {rule.JoinClause}
                WHERE {rule.ConditionClause}
            ";

            var results = await con.QueryAsync<SmsResult>(query);

            foreach (var item in results)
            {
                await SendSmsAsync(item.MobileNo, rule.TemplateMessage);
            }
        }
    }

    public class SmsRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public string SelectClause { get; set; }
        public string FromTable { get; set; }
        public string JoinClause { get; set; }
        public string ConditionClause { get; set; }
        public string TemplateMessage { get; set; }
    }

    public class SmsResult
    {
        public int PatientId { get; set; }
        public string MobileNo { get; set; }
    }

    public async Task SendSmsAsync(string mobileNo, string messageTemplate)
    {
        // Basic mobile number validation (10-15 digits, can be adjusted as needed)
        if (string.IsNullOrWhiteSpace(mobileNo)
            || !System.Text.RegularExpressions.Regex.IsMatch(mobileNo, "^[1-9][0-9]{9,14}$")
            || System.Text.RegularExpressions.Regex.IsMatch(mobileNo, "^0+$"))
        {
            Console.WriteLine($"[ERROR] Invalid mobile number: {mobileNo}");
            return;
        }
        try
        {
            var twilioConfig = GetTwilioConfigFromDb(_conn);
            if (twilioConfig == null)
            {
                Console.WriteLine("[ERROR] Twilio configuration is missing.");
                return;
            }

            string accSid = twilioConfig.AccountSid;
            string authToken = twilioConfig.AuthToken;
            string serviceSid = twilioConfig.MessagingServiceSid;
            string CountryCode = twilioConfig.CountryCode;

            TwilioClient.Init(accSid, authToken);
            var fullPhoneNumber = $"{CountryCode}{mobileNo}";
            var msgOptions = new CreateMessageOptions(new PhoneNumber(fullPhoneNumber))
            {
                MessagingServiceSid = serviceSid,
                Body = messageTemplate
            };
            var message = await MessageResource.CreateAsync(msgOptions);

            Console.WriteLine($"[SMS SENT] To: {fullPhoneNumber}, MessageSid: {message.Sid}, Status: {message.Status}, Body: {msgOptions.Body}");
            // Optionally log to DB if needed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to send SMS to: {mobileNo}. Error: {ex.Message}");
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