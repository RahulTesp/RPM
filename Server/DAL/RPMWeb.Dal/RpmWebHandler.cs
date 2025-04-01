using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RPMWeb.Dal
{
    public class DeviceRegisterRequest
    {
        public string device_id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int time_zone { get; set; }
    }
    public class RpmWebHandler
    {
        public HttpResponse SendRegisterRequest(DeviceRegisterRequest drv, List<SystemConfigInfo> igc, ref int devuserid)
        {
            HttpResponse retCode = new HttpResponse();  
            try
            {
                SystemConfigInfo acccreate = igc.Find(x => x.Name.Equals("UrlAccCreate_iGlucose"));
                SystemConfigInfo apikey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
                if (acccreate == null || apikey == null)
                {
                    throw new Exception("Invalid Configurations.");
                }
                string urlCreateAcc = acccreate.Value + "=" + apikey.Value;
                

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(drv);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(urlCreateAcc, data).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(result);
                    retCode.HttpRetCode = (int)obj["status"]["status_code"];
                    retCode.Message = (string)obj["status"]["status_message"];
                    if (retCode.HttpRetCode == 201)
                    {
                        devuserid = (int)obj["account"]["user_id"];
                        
                    }
                }

            }
            catch
            {
                throw;
            }
            return retCode;
        }
        public RegisterDeviceResponse AddDeviceRequest(string deviceid, string deviceuserid, string devModel, List<SystemConfigInfo> igc)
        {
            RegisterDeviceResponse ret = new RegisterDeviceResponse();
            try
            {
                SystemConfigInfo acccreate = igc.Find(x => x.Name.Equals("UrlAddDevice_iGlucose"));
                SystemConfigInfo apikey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
                if (acccreate == null || apikey == null)
                {
                    throw new Exception("Invalid Configurations.");
                }
                string urlCreateAcc = string.Format(acccreate.Value, apikey.Value, deviceid, devModel, deviceuserid);

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(urlCreateAcc).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(result);
                    int retCode = (int)obj["status"]["status_code"];
                    ret.HttpRetCode = retCode;
                    ret.DevicUserId = deviceuserid;
                    if (retCode == 200)
                    {
                        ret.Message = "Device added successfully";
                    }
                    else
                    {
                        ret.Message = "Device add operation failed";
                    }
                }

            }
            catch
            {
                throw;
            }
            return ret;
        }
        public bool ValidateDeviceRequest(string deviceid, List<SystemConfigInfo> igc)
        {
            bool ret = false;
            SystemConfigInfo urlValidate = igc.Find(x => x.Name.Equals("UrlValidateDevice_iGlucose"));
            SystemConfigInfo apikey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
            if (urlValidate == null || apikey == null)
            {
                throw new Exception("Invalid Configurations.");
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    JObject obj = new JObject();
                    obj["api_key"] = apikey.Value;
                    obj["device_id"] = deviceid;
                    var json = JsonConvert.SerializeObject(obj);

                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(urlValidate.Value, data).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject objResult = JsonConvert.DeserializeObject<JObject>(result);
                    if(objResult != null)
                    {
                        int statcode = (int)objResult["status"]["status_code"];
                        if(statcode == 200)
                        {
                            ret = (bool)objResult["is_valid"];
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
        public RegisterDeviceResponse RemoveDeviceRequest(string deviceid, string deviceuserid, string devModel, List<SystemConfigInfo> igc)
        {
            RegisterDeviceResponse ret = new RegisterDeviceResponse();
            try
            {
                SystemConfigInfo urlValidate = igc.Find(x => x.Name.Equals("UrlRemoveDevice_iGlucose"));
                SystemConfigInfo apikey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
                if (urlValidate == null || apikey == null)
                {
                    throw new Exception("Invalid Configurations.");
                }
                string urlCreateAcc = string.Format(urlValidate.Value, apikey.Value, deviceid, devModel, deviceuserid);
               
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(urlCreateAcc).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(result);
                    int retCode = (int)obj["status"]["status_code"];
                    ret.HttpRetCode = retCode;
                    ret.DevicUserId = deviceuserid;
                    if (retCode == 200)
                    {
                        ret.Message = "Device removed successfully";
                    }
                    else
                    {
                        ret.Message = "Device remove operation failed";
                    }
                }

            }
            catch
            {
                throw;
            }
            return ret;
        }
    }
}
