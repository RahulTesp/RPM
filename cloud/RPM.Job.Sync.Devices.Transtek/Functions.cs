using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using RPMWeb.Data.Common;
using System.Data;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace SyncTranstekDevices
{
    public class Functions
    {
        public static string? SERVER { get; set; }
        public static string? USER { get; set; }
        public static string? PWD { get; set; }
        public static string? API_KEY { get; set; }
        public static string? SMTP_ToMail { get; set; }
        public static string? SMTP { get; set; }
        public static string? VALIDATE_URL { get; set; }
        public static string? DEVICE_URL { get; set; }
        public static string? CONN_STRING { get; set; }
        public static string? TKN { get; set; }
        public static string? DeviceModel { get; set; }
        public static string? DeviceIMEI { get; set; }
        public static string? DeviceType { get; set; }
        public static int AddedCount { get; set; }
        public static int FailedCount { get; set; }
        public static int ExistingCount { get; set; }
        public static int TotalCount { get; set; }

        public static void ConfigureSettings()
        {
            // Set up configuration
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables() // Allows overriding via Azure App Settings
            .Build();
            if (config == null)
            {
                Console.WriteLine("Configuration is null.");
                return;
            }
            // Access a specific config value
            string? connStr = config["RPM:ConnectionString"];
            Console.WriteLine($"RPM Connection String: {connStr}");
            if (connStr == null)
            {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            CONN_STRING = connStr;
            Console.WriteLine("WebJob started...");
            if (CONN_STRING == null)
            {
                Console.WriteLine("Connection string is null.");
                return;
            }
            SERVER = config["RPM:Server"];
            USER = config["RPM:UserName"];
            PWD = config["RPM:Password"];
            API_KEY = config["RPM:APIKey"];
            SMTP_ToMail = config["RPM:SMTP_ToMail"];
            SMTP = config["RPM:SMTP"];
            VALIDATE_URL = config["RPM:validate_url"];
            DEVICE_URL = config["RPM:devices_url"];

        }
        public static void SyncDevices()
        {
            ConfigureSettings();
            string msg;
            try
            {
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
                if (deviceList != null)
                {
                    TotalCount = 0;
                    foreach (var device in deviceList)
                    {

                        TotalCount = TotalCount + 1;
                        Console.WriteLine("Device - " + device["deviceId"]);
                        string deviceStatus = ValidateDeviceRequest((string)device["deviceId"], VALIDATE_URL);
                        Console.WriteLine("Device Validate Status - " + deviceStatus);
                        Console.WriteLine("Device Model - " + device["modelNumber"]);

                        ValidateDeviceModel((string)device["modelNumber"]);
                        var isDeviceAvailable = ValidateDeviceRPM((string)device["deviceId"], (string)device["modelNumber"]);
                        if (isDeviceAvailable)
                        {
                            Console.WriteLine("Device Not Available in RPM");
                            var DevAddStatus = SyncDeviceRPM((string)device["deviceId"], (string)device["modelNumber"]);
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
                    Console.WriteLine("Devices fetched successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to fetch devices.");
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
                //sendEmail(msg);
                return;
            }

        }
        public static JArray getDeviceList()
        {
            JArray ret = new JArray();
            try
            {
                if (DEVICE_URL == null || TKN == null)
                {
                    throw new Exception("Invalid Device List Configurations.");
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("x-api-key", API_KEY);
                    HttpResponseMessage response = client.GetAsync(DEVICE_URL).Result;
                    response.EnsureSuccessStatusCode();
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                    if (objResult != null)
                    {
                        if ((int)response.StatusCode == 200)
                        {
                            var items = objResult["items"] as JArray;
                            if (items != null)
                            {
                                JArray completeDevices = new JArray();
                                foreach (var item in items)
                                {
                                    string retStatus = (string)item["status"];

                                    if (retStatus == "complete")
                                    {
                                        completeDevices.Add(item);
                                    }
                                }
                                if (completeDevices.Count > 0)
                                {
                                    ret = completeDevices;
                                }
                                else
                                {
                                    Console.WriteLine("No devices with 'complete' status found.");
                                    ret = null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("No items found.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error deserializing JSON response.");
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }

        public static string ValidateDeviceRequest(string deviceid, string url)
        {
            string ret = string.Empty;
            if (deviceid == null || url == null)
            {
                throw new Exception("Invalid Configurations.");
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    JObject obj = new JObject();
                    obj["x-api-key"] = API_KEY;
                    obj["device_id"] = deviceid;
                    var json = JsonConvert.SerializeObject(obj);

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("x-api-key", API_KEY);

                    HttpResponseMessage response = client.GetAsync(url + deviceid).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                    if (objResult != null)
                    {
                        string retStatus = (string)objResult["status"];

                        if (retStatus == "complete")
                        {
                            ret = (string)objResult["status"];
                            DeviceModel = objResult["modelNumber"].ToString();
                            DeviceIMEI = objResult["imei"].ToString();
                            DeviceType = GetDeviceType(DeviceModel);
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
        public static string GetDeviceType(string deviceModel)
        {
            string url = "https://" + SERVER + "/api/device/getDeviceType?deviceModel=" + Uri.EscapeDataString(deviceModel);
            if (url == null)
            {
                throw new Exception("Invalid Configurations.");
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Bearer", TKN);
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return result.Trim('"');
                    }
                    else if ((int)response.StatusCode == 404)
                    {
                        return null;
                    }
                    else
                    {
                        throw new Exception("API error: " + response.StatusCode + " - " + result);
                    }
                }
            }
            catch
            {
                throw;
            }
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
                    obj["DeviceVendor"] = "TRANSTEK";
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
                    obj["Name"] = deviceConfigjson[DeviceType]["Name"];
                    obj["DeviceType"] = DeviceType;
                    obj["DeviceModel"] = deviceModel;
                    obj["PatientId"] = 0;
                    obj["DeviceVendor"] = "TRANSTEK";
                    obj["DeviceManufacturer"] = "lifesense";
                    obj["DeviceNumber"] = "TM" + deviceid;
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

        public static string getDeviceConfig()
        {
            var model_config = "{'Blood Pressure Monitor':{\"Name\":\"Blood Pressure Monitoring Device\"},'Pulse Oximeter':{\"Name\":\"Oxygen Monitoring Device\"},'Body Weight Monitor':{\"Name\":\"Body Weight Monitoring Device\"},'Blood Glucose Monitor':{\"Name\":\"Blood Glucose Monitoring Device\"}}";
            return model_config;
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
            catch
            {
                throw;
            }
            return contactDetails;
        }

        public static string sendEmail(string body)
        {
            ContactDetails contactDetails = new ContactDetails();
            contactDetails = GetEmailDetails();
            string toEmail = SMTP_ToMail;
            string subject = "Sync Device Job Failure";

            SmtpClient smtpClient = new SmtpClient(SMTP, 587);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(contactDetails.FromMail, contactDetails.Password);

            MailMessage mailMessage = new MailMessage(contactDetails.FromMail, toEmail, subject, body);

            try
            {
                smtpClient.Send(mailMessage);
                return "Email Sent";
            }
            catch (Exception ex)
            {
                return "Failed to Send Email - " + ex;
            }
        } 
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
}
