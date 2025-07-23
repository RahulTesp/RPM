using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RPMWeb.Data.Common;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//cron 0 0 7 * * *
class Program
{
    static string CONN_STRING =string.Empty;
    public static string API_KEY = string.Empty;
    public static string USER = string.Empty;
    public static string PWD = string.Empty;
    public static string DEVICE_URL = string.Empty;
    public static string VALIDATE_URL = string.Empty;
    public static string SERVER = string.Empty;
    public static string SMTP = string.Empty;
    public static string SMTP_ToMail = string.Empty;

    public static string TKN { get; set; }
    public static string DeviceModel { get; set; }
    public static string DeviceIMEI { get; set; }
    public static string DeviceType { get; set; }
    public static int AddedCount { get; set; }
    public static int FailedCount { get; set; }
    public static int ExistingCount { get; set; }
    public static int TotalCount { get; set; }
    static async Task Main(string[] args)
    {
        string msg;
        try
        {
            // Set up configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables() // Allows overriding via Azure App Settings
                .Build();
            // Optional: bind strongly-typed object
            var rpmSettings = config.GetSection("RPM").Get<RpmSettings>();
            Console.WriteLine($"RPM.ConnectionString (typed): {rpmSettings?.ConnectionString}");
            CONN_STRING = rpmSettings?.ConnectionString;
            API_KEY = rpmSettings?.APIKey;
            USER = rpmSettings?.UserName;
            PWD = rpmSettings?.Password;
            DEVICE_URL = rpmSettings?.devices_url;
            VALIDATE_URL = rpmSettings?.validate_url;
            SERVER = rpmSettings?.Server;
            SMTP = rpmSettings?.SMTP;
            SMTP_ToMail = rpmSettings?.SMTP_ToMail;
            Console.WriteLine("WebJob started...");
            if (CONN_STRING == null)
            {
                Console.WriteLine("Connection string is null.");
                return;
            }
            AddedCount = 0;
            FailedCount = 0;
            ExistingCount = 0;
            var rpm_login_url = "https://" + SERVER + "/api/authorization/Userlogin";
            var login_data = new login();
            LoginResponse login_resp = new LoginResponse();
            login_resp = LoginRPM(USER, PWD, rpm_login_url);
            TKN = login_resp.tkn;
            if (TKN == null)
            {
                msg = "Login Failed for Sync Device Job, Please check Login Credentials. at server  " + SERVER;
                sendEmail(msg);
                return;
            }
            var deviceList = getDeviceList();
            var devices = deviceList["devices"].Values();
            if (devices != null)
            {
                TotalCount = 0;
                foreach (string device in devices)
                {

                    TotalCount = TotalCount + 1;
                    Console.WriteLine("Device - " + device);
                    bool deviceStatus = ValidateDeviceRequest(device, VALIDATE_URL);
                    Console.WriteLine("Device Validate Status - " + deviceStatus);
                    Console.WriteLine("Device Model - " + DeviceModel);
                    if (deviceStatus)
                    {
                        ValidateDeviceModel(DeviceModel);
                        var isDeviceAvailable = ValidateDeviceRPM(device, DeviceModel);
                        if (isDeviceAvailable)
                        {
                            Console.WriteLine("Device Not Available in RPM");
                            var DevAddStatus = SyncDeviceRPM(device, DeviceModel);
                            if (DevAddStatus)
                            {
                                Console.WriteLine(device + " - Added Successfully..!");
                                AddedCount++;
                            }
                            else
                            {
                                Console.WriteLine(device + " - Failed to Add..!");
                                FailedCount++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Device already Available in RPM");
                            ExistingCount = ExistingCount + 1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Device validation failed in iGlucose");
                    }
                }
            }
            Console.WriteLine("Added Device Count - " + AddedCount);
            Console.WriteLine("Failed Device Count - " + FailedCount);
            Console.WriteLine("Existing Device Count - " + ExistingCount);
            Console.WriteLine("Total Device Count - " + TotalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            msg = "Sync Device Job Failed at " + SERVER + " " + ex;
            sendEmail(msg);
            return;
        }


    }
    public static JObject getDeviceList()
    {
        JObject ret = new JObject();
        try
        {
            if (DEVICE_URL == null || TKN == null)
            {
                throw new Exception("Invalid Device List Configurations.");
            }

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(DEVICE_URL + API_KEY).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                if (objResult != null)
                {
                    if ((int)response.StatusCode == 200)
                    {
                        int retCode = (int)objResult["status"]["status_code"];
                        if (retCode == 200)
                        {
                            ret = objResult;
                        }
                        else
                        {
                            ret = null;
                        }
                    }
                }
            }

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        return ret;
    }

    public static bool ValidateDeviceRequest(string deviceid, string url)
    {
        bool ret = false;
        if (deviceid == null || url == null)
        {
            throw new Exception("Invalid Configurations.");
        }
        try
        {
            using (HttpClient client = new HttpClient())
            {
                JObject obj = new JObject();
                obj["api_key"] = API_KEY;
                obj["device_id"] = deviceid;
                var json = JsonConvert.SerializeObject(obj);

                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, data).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                if (objResult != null)
                {
                    int statcode = (int)objResult["status"]["status_code"];
                    if (statcode == 200)
                    {
                        ret = (bool)objResult["is_valid"];
                        DeviceModel = objResult["device_model"].ToString();
                        DeviceIMEI = objResult["imei"].ToString();
                        DeviceType = objResult["device_type"].ToString();
                    }
                }
            }
        }
        catch
        {
            throw;
        }
        return ret;
    }

    public static bool ValidateDeviceRPM(string deviceid, string deviceModel)
    {
        string url = "https://" + SERVER + "/api/device/isdeviceavailable";
        bool ret = false;
        if (deviceid == null || url == null)
        {
            throw new Exception("Invalid Configurations.");
        }
        try
        {
            using (HttpClient client = new HttpClient())
            {
                JObject obj = new JObject();
                obj["DeviceId"] = deviceid;
                obj["DeviceModel"] = deviceModel;
                obj["DeviceVendor"] = "iGlucose";
                var json = JsonConvert.SerializeObject(obj);
                client.DefaultRequestHeaders.Add("Bearer", TKN);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, data).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                if (result != null)
                {
                    if ((int)response.StatusCode == 404)
                    {
                        ret = true;

                    }
                    else
                    {
                        ret = false;
                    }
                }
            }
        }
        catch
        {
            throw;
        }
        return ret;
    }

    public static bool ValidateDeviceModel(string deviceModel)
    {
        string url = "https://" + SERVER + "/api/device/isdeviceModelavailable";
        bool ret = false;
        if (url == null)
        {
            throw new Exception("Invalid Configurations.");
        }
        try
        {
            using (HttpClient client = new HttpClient())
            {
                JObject obj = new JObject();
                obj["DeviceModel"] = deviceModel;
                var json = JsonConvert.SerializeObject(obj);
                client.DefaultRequestHeaders.Add("Bearer", TKN);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, data).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                if (result != null)
                {
                    if ((int)response.StatusCode == 404)
                    {
                        ret = true;

                    }
                    else
                    {
                        ret = false;
                    }
                }
            }
        }
        catch
        {
            throw;
        }
        return ret;
    }

    public static bool SyncDeviceRPM(string deviceid, string deviceModel)
    {
        string url = "https://" + SERVER + "/api/device/adddevice";
        string deviceConfig = getDeviceConfig();
        JObject deviceConfigjson = JObject.Parse(deviceConfig);
        bool ret = false;
        if (deviceid == null || url == null)
        {
            throw new Exception("Invalid Configurations.");
        }
        try
        {
            using (HttpClient client = new HttpClient())
            {
                JObject obj = new JObject();
                if (deviceModel == "LS802-GA")
                {

                }
                obj["Name"] = deviceConfigjson[DeviceType]["Name"];
                obj["DeviceType"] = deviceConfigjson[DeviceType]["DeviceType"];
                obj["DeviceModel"] = deviceModel;
                obj["PatientId"] = 0;
                obj["DeviceVendor"] = "iGlucose";
                obj["DeviceManufacturer"] = "iGlucose";
                obj["DeviceNumber"] = "IG" + deviceid;
                obj["DeviceSerialNo"] = deviceid;
                obj["DeviceIMEINo"] = DeviceIMEI;
                obj["DeviceStatus"] = "Available";
                obj["DeviceCommunicationType"] = "LTE";
                obj["City"] = "Jamesburg";

                var json = JsonConvert.SerializeObject(obj);
                client.DefaultRequestHeaders.Add("Bearer", TKN);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, data).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                if (result != null)
                {
                    if ((int)response.StatusCode == 200 || (int)response.StatusCode == 201)
                    {
                        ret = true;

                    }
                    else
                    {
                        ret = false;
                    }
                }
            }
        }
        catch
        {
            throw;
        }
        return ret;
    }


    public static LoginResponse LoginRPM(string User, string Password, string url)
    {
        LoginResponse ret = new LoginResponse();
        try
        {
            if (User == null || Password == null)
            {
                throw new Exception("Invalid Login.");
            }

            using (HttpClient client = new HttpClient())
            {
                JObject obj = new JObject();
                obj["UserName"] = User;
                obj["Password"] = Password;
                var json = JsonConvert.SerializeObject(obj);

                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, data).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                if (objResult != null)
                {
                    if ((int)response.StatusCode == 200)
                    {
                        ret.tkn = (string)objResult["tkn"];
                    }
                }
            }

        }
        catch
        {
            throw;
        }
        return ret;
    }

    public static object httpGetDeviceData(string url, device Reqdata, string tkn = null)
    {
        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(Reqdata);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return result;
                }
                else
                {
                    return null;
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());   
            throw;

        }

    }
    public static string getDeviceConfig()
    {
        var model_config = "{'blood_pressure':{\"Name\":\"Blood Pressure Monitoring Device\",\"DeviceType\":\"Blood Pressure Monitor\"},'pulse_ox':{\"Name\":\"Oxygen Monitoring Device\",\"DeviceType\":\"Pulse Oximeter\"},'weight':{\"Name\":\"Body Weight Monitoring Device\",\"DeviceType\":\"Body Weight Monitor\"},'blood_glucose':{\"Name\":\"Blood Glucose Monitoring Device\",\"DeviceType\":\"Blood Glucose Monitor\"}}";
        return model_config;
    }

    public static ContactDetails GetEmailDetails()
    {
        ContactDetails contactDetails = new ContactDetails();
        List<Roles> Listinfo = new List<Roles>();
        try
        {

            using (SqlConnection connection = new SqlConnection(CONN_STRING))
            {
                SqlCommand command = new SqlCommand("usp_GetEmailDetails", connection);
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    contactDetails.FromMail = Convert.ToString(reader["FromEmail"]);
                    contactDetails.Password = Convert.ToString(reader["Password"]);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        return contactDetails;
    }
    public static string sendEmail(string body)
    {
        try
        {
            ContactDetails contactDetails = new ContactDetails();
            contactDetails = GetEmailDetails();
            string toEmail = SMTP_ToMail;
            string subject = "Sync Device Job Failure";
            SmtpClient smtpClient = new SmtpClient(SMTP, 587);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(contactDetails.FromMail, contactDetails.Password);
            MailMessage mailMessage = new MailMessage(contactDetails.FromMail, toEmail, subject, body);
            smtpClient.Send(mailMessage);
            return "Email Sent";


        }
        catch (Exception ex)
        {
            return "Failed to Send Email - " + ex;

        }
    }

}
public class RpmSettings
{
    public string? ConnectionString { get; set; }
    public string? Server { get; set; }
    public string? validate_url { get; set; }
    public string? devices_url { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? APIKey { get; set; }
    public string? SMTP_ToMail { get; set; }
    public string? SMTP { get; set; }
}
public class device
{
    public string api_key;
    public string device_id;
}

public class login
{
    public string UserName;
    public string Password;
}

public class LoginResponse : HttpResponse
{
    public string tkn { get; set; }
}
