using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Globalization;
//cron continuous

namespace azuretranstekwebjob
{
    internal class Program
    {
        static string CONN_STRING = string.Empty;
        static ConcurrentDictionary<string, DeviceIDs> deviceid_dictionary = new ConcurrentDictionary<string, DeviceIDs>();
        static ConcurrentDictionary<string, string> vitalunits_dictionary = new ConcurrentDictionary<string, string>();
        static double bg_fastingStartTime = 0;
        static double bg_fastingEndTime = 0;
        static async Task Main(string[] args)
        {
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
            if (connStr == null)
            {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            if (CONN_STRING == null)
            {
                Console.WriteLine("Connection string is null.");
                return;
            }
            CONN_STRING = connStr;
            // Parse connection string for server and database info
            var builder = new DbConnectionStringBuilder { ConnectionString = connStr };
            string server = builder.ContainsKey("Server") ? builder["Server"].ToString() : "";
            string database = builder.ContainsKey("Initial Catalog") ? builder["Initial Catalog"].ToString() : "";

            Console.WriteLine($"Server: {server}");
            Console.WriteLine($"Database: {database}");

            Console.WriteLine("WebJob started...");
           
            Thread.Sleep(10000);
            while (true)
            {
                try
                {
                    TimerDeviceIdCallback();
                    Thread.Sleep(1000);
                    TimerApiCallback();
                    Thread.Sleep(20000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception:" + ex);
                }
            }
            
        }
        private static void TimerDeviceIdCallback()
        {
            try
            {

                Dictionary<string, DeviceIDs> temp_deviceid_dictionary = new Dictionary<string, DeviceIDs>();
                Console.WriteLine("Reading DeviceId and Vitals");
                using (SqlConnection connection = new SqlConnection(CONN_STRING))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("usp_GetDeviceIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@deviceVendorName", "TRANSTEK");
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var Deviceid = reader["DeviceSerialNo"].ToString();
                            var ActivatedDate = (DateTime)reader["DeviceActivatedDateTime"];
                            temp_deviceid_dictionary.Add(Deviceid, new DeviceIDs(Deviceid, ActivatedDate));
                            deviceid_dictionary.TryAdd(Deviceid, new DeviceIDs(Deviceid, ActivatedDate));
                            foreach (var item in deviceid_dictionary)
                            {
                                if (!temp_deviceid_dictionary.ContainsKey(item.Key))
                                {
                                    DeviceIDs removedItem;
                                    bool result = deviceid_dictionary.TryRemove(item.Key, out removedItem);
                                }
                                else
                                {
                                    deviceid_dictionary[Deviceid] = new DeviceIDs(Deviceid, ActivatedDate);
                                }
                            }
                        }
                    }
                    SqlCommand command1 = new SqlCommand("usp_GetVitalUnits", connection);
                    command1.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var MeasureName = reader["MeasureName"].ToString();
                            var Unit = reader["UnitName"].ToString();
                            vitalunits_dictionary.TryAdd(MeasureName, Unit);
                        }
                    }
                }
                List<SystemConfigInfo> config = GetSystemConfig(CONN_STRING, "TRANSTEK", "TranstekJob");
                SystemConfigInfo? startTimeKey = config.Find(x => x.Name.Equals("BG_FastingStartTime"));
                bg_fastingStartTime =  Convert.ToDouble( startTimeKey?.Value);
                SystemConfigInfo? endTimeKey = config.Find(x => x.Name.Equals("BG_FastingEndTime"));
                bg_fastingEndTime = Convert.ToDouble(endTimeKey?.Value);
                Console.WriteLine("Reading DeviceId and Vitals end");
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex);
            }
        }

        private static void TimerApiCallback()
        {

            Console.WriteLine("TimerApiCallback begin");
            try
            {
                foreach (var item in deviceid_dictionary)
                {

                    DeviceIDs val = item.Value;
                    DateTime lastRecodDate = val.ActivatedDate;

                    Console.WriteLine("DeviceId: " + val.Deviceid + "; Last Recorded :" + lastRecodDate);
                    DateTime newstart = lastRecodDate.AddSeconds(2);

                    List<KeyValuePair<DateTime, string>> sdata = GetJsonFromStagingTable(val.Deviceid, newstart);

                    if (sdata == null || sdata.Count <= 0)
                    {
                        DateTime newenddate1 = newstart.AddDays(2);
                        if (newenddate1 <  DateTime.Now)
                        {
                            UpdateAppJobLastBatch(val.Deviceid, newenddate1);
                        }                                               
                        continue;
                    }
                        
                    
                    string deviceType = IsValidlifesenseDevice(val.Deviceid);
                    if (string.IsNullOrEmpty(deviceType))
                    {
                        Console.WriteLine("Device type not valid or not found.");
                        continue;
                    }
                    List<DateTime> receivedTimes = new List<DateTime>();
                        foreach (var readingItem in sdata)
                        {
                            try
                            {
                                // Each item is a JSON-encoded string, so parse it again
                                TranstekDeviceTelemetry telemetry = JsonConvert.DeserializeObject<TranstekDeviceTelemetry>(readingItem.Value);

                                if (telemetry != null)
                                {

                                    //if (telemetry.messageType == "telemetry")
                                    //{

                                        if (telemetry.data == null || telemetry.data.ToString() == "{}")
                                        {
                                            continue;
                                        }
                                        bool insertSuccess = StagingTableQueueInsert(telemetry, deviceType);

                                        if (insertSuccess)
                                        {
                                            Console.WriteLine($"Successfully inserted reading for deviceId: {telemetry}");
                                            // Track createdAt timestamp
                                            receivedTimes.Add(readingItem.Key);
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Failed to insert reading for deviceId: {telemetry.deviceId}");
                                        }
                                    //}
                                    //else
                                    //{
                                    //    Console.WriteLine($"Skipping non-telemetry messageType: {telemetry.messageType}");
                                    //}
                                }
                                else
                                {
                                    Console.WriteLine("Deserialized telemetry object is null.");
                                    continue;
                                }
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Error processing individual reading: {innerEx.Message}");
                            }
                        }
                    DateTime newenddate = newstart.AddDays(2);
                    if (newenddate < DateTime.Now)
                    {
                        UpdateAppJobLastBatch(val.Deviceid, newenddate);
                    }
                    else
                    {
                        if (receivedTimes.Any())
                        {
                            DateTime maxReceivedTime = receivedTimes.Max();
                            UpdateAppJobLastBatch(val.Deviceid, maxReceivedTime);
                        }
                    }
                    // After processing all readings, update last batch if we inserted any
                    
                }
                Console.WriteLine("TimerApiCallback end");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception before StagingTableQueueInsert:" + ex);
            }
        }
        public static List<SystemConfigInfo> GetSystemConfig(string cs, string category, string createdby)
        {
            List<SystemConfigInfo> ret = new List<SystemConfigInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    //string query = "select * from SystemConfigurations where Category='iGlucose'";
                    SqlCommand command = new SqlCommand("usp_GetSystemConfig", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Category", SqlDbType.NVarChar).Value = category;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = createdby;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SystemConfigInfo info = new SystemConfigInfo();
                        info.Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader["name"].ToString() : string.Empty;
                        info.Value = !reader.IsDBNull(reader.GetOrdinal("Value")) ? reader["Value"].ToString() : string.Empty;
                        info.Descripiton = !reader.IsDBNull(reader.GetOrdinal("Description")) ? reader["Description"].ToString() : string.Empty;
                        ret.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        private static void UpdateAppJobLastBatch(string Deviceid, DateTime newenddate)
        {
            if (newenddate <= DateTime.Now)
            {
                using (SqlConnection connection = new SqlConnection(CONN_STRING))
                {
                    SqlCommand command = new SqlCommand("usp_AppJobLastBatch", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceSerialNo", Deviceid);
                    command.Parameters.AddWithValue("@DeviceVendorName", "TRANSTEK");
                    command.Parameters.AddWithValue("@DateRecorded", newenddate);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id != 0)
                    {
                        Console.WriteLine(Deviceid + " Last Record Processed Date: " + newenddate);
                    }

                }
            }
        }
        private static List<KeyValuePair<DateTime, string>> GetJsonFromStagingTable(string deviceId, DateTime startDate)
        {
            var results = new List<KeyValuePair<DateTime, string>>();
            try
            {
                DateTime newenddate = startDate.AddDays(2);
                if (newenddate >= DateTime.Now)
                {
                    newenddate = DateTime.Now.ToUniversalTime();
                }

                string strstartdate = startDate.ToString("yyyy-MM-dd'T'HH:mm:ss");
                strstartdate = strstartdate.Replace('.', ':');
                string strnewenddate = newenddate.ToString("yyyy-MM-dd'T'HH:mm:ss");
                strnewenddate = strnewenddate.Replace('.', ':');
                List<string> jsonReadings = new List<string>();

                using (SqlConnection connection = new SqlConnection(CONN_STRING))
                {
                    SqlCommand command = new SqlCommand("usp_GetTranstekJsonDataByDeviceAndDate", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceId", deviceId);
                    command.Parameters.AddWithValue("@StartDate", strstartdate);
                    command.Parameters.AddWithValue("@EndDate", strnewenddate);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object obj = reader["createdOn"];
                            DateTime createdOn = obj != DBNull.Value ? ((DateTime)obj).ToUniversalTime() : DateTime.MinValue;
                            var jsonValue = reader["Json"];
                            string? jsonString = jsonValue?.ToString();
                            if (jsonString != null)
                            {
                                KeyValuePair<DateTime, string> valuePair = new KeyValuePair<DateTime, string>(
                                    createdOn,
                                    jsonString
                                );

                                results.Add(valuePair); // raw JSON string
                            }
                            // If jsonString is null, skip adding to results
                        }
                    }
                }
                return results;


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
        }
        private static string IsValidlifesenseDevice(string DeviceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(CONN_STRING))
                {
                    SqlCommand command = new SqlCommand("usp_GetDeviceManufacturer", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceId", DeviceId);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    string Manufacturer = "";
                    string DeviceType = "";
                    while (reader.Read())
                    {
                        Manufacturer = reader["Manufacturer"].ToString();
                        DeviceType = reader["DeviceType"].ToString();
                    }
                    connection.Close();
                    if (Manufacturer == "lifesense")
                    {
                        return DeviceType;
                    }
                    return null;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static bool StagingTableQueueInsert(TranstekDeviceTelemetry dev, string deviceType)
        {
            GetVitalUnits();
            bool ret = false;
            string stagingInsert = "INSERT INTO JsonStg([Json])VALUES";
            deviceType = deviceType.Trim();
            if (deviceType == "Blood Pressure Monitor")
            {
                ret = ProcessBloobPressureData(dev, CONN_STRING, stagingInsert);
            }
            else if (deviceType == "Blood Glucose Monitor")
            {
                ret = ProcessBloodGlucoseData(dev, CONN_STRING, stagingInsert);
            }
            else if (deviceType == "Body Weight Monitor")
            {
                ret = ProcessWeightData(dev, CONN_STRING, stagingInsert);
            }
            else if (deviceType == "Pulse Oximeter")
            {
                ret = ProcessOxygenData(dev, CONN_STRING, stagingInsert);
            }
            return ret;
        }
        private static void GetVitalUnits()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CONN_STRING))
                {
                    con.Open();
                    SqlCommand command1 = new SqlCommand("usp_GetVitalUnits", con);
                    command1.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var MeasureName = reader["MeasureName"].ToString();
                            var Unit = reader["UnitName"].ToString();
                            vitalunits_dictionary.TryAdd(MeasureName, Unit);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static int GetTimeZoneOffset(string tz)
        {
            string tzString = tz.Trim().ToLower();
            int timeZoneOffset = 0;

            if (tzString.StartsWith("utc"))
            {
                tzString = tzString.Substring(3);
            }

            if (int.TryParse(tzString, out timeZoneOffset))
            {
                return timeZoneOffset;
            }
            return 0;
        }
        public static DateTime ConvertUnixTimestampToDateTime(long timestamp)
        {
            DateTime dateTimeRx;
            if (timestamp.ToString().Length == 10)
            {
                dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
            else if (timestamp.ToString().Length == 13)
            {
                dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(timestamp / 1000).DateTime;
            }
            else
            {
                throw new ArgumentException("Invalid timestamp length.");
            }

            return dateTimeRx;
        }
        private static bool ProcessBloobPressureData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Systolic", out string Unitsystolic);
                vitalunits_dictionary.TryGetValue("Diastolic", out string Unitdiastolic);
                vitalunits_dictionary.TryGetValue("Pulse", out string Unitpulse);
                if (dev.data == null)
                    return false;
                JObject obj = JObject.Parse(dev.data.ToString());
                StagingInput blood_pressuresystolic = new StagingInput();
                StagingInput blood_pressurediastolic = new StagingInput();
                StagingInput blood_pressurepulse = new StagingInput();
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                blood_pressuresystolic.reading_id = readingIdWithPrefix;
                blood_pressurediastolic.reading_id = readingIdWithPrefix;
                blood_pressurepulse.reading_id = readingIdWithPrefix;
                blood_pressuresystolic.device_id = dev.deviceId;
                blood_pressurediastolic.device_id = dev.deviceId;
                blood_pressurepulse.device_id = dev.deviceId;
                blood_pressuresystolic.device_model = dev.modelNumber;
                blood_pressurediastolic.device_model = dev.modelNumber;
                blood_pressurepulse.device_model = dev.modelNumber;
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                blood_pressuresystolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                blood_pressurediastolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                blood_pressurepulse.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //blood_pressuresystolic.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //blood_pressurediastolic.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //blood_pressurepulse.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_pressuresystolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                blood_pressurediastolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                blood_pressurepulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                blood_pressuresystolic.reading_type = "blood_pressure";
                blood_pressurediastolic.reading_type = "blood_pressure";
                blood_pressurepulse.reading_type = "blood_pressure";
                blood_pressuresystolic.battery = (int)obj["bat"];
                blood_pressurediastolic.battery = (int)obj["bat"];
                blood_pressurepulse.battery = (int)obj["bat"];
                var time = obj["tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    blood_pressuresystolic.time_zone_offset = timeZone.ToString();
                    blood_pressurediastolic.time_zone_offset = timeZone.ToString();
                    blood_pressurepulse.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    blood_pressuresystolic.time_zone_offset = null;
                    blood_pressurediastolic.time_zone_offset = null;
                    blood_pressurepulse.time_zone_offset = null;
                }
                blood_pressuresystolic.before_meal = false;
                blood_pressurediastolic.before_meal = false;
                blood_pressurepulse.before_meal = false;
                blood_pressuresystolic.event_flag = null;
                blood_pressurediastolic.event_flag = null;
                blood_pressurepulse.event_flag = null;
                blood_pressuresystolic.irregular = false;
                blood_pressurediastolic.irregular = false;
                blood_pressurepulse.irregular = false;
                blood_pressuresystolic.data_type = "systolic";
                blood_pressuresystolic.data_unit = Unitsystolic;
                blood_pressuresystolic.data_value = Math.Round(Convert.ToDouble(obj["sys"]),2);
                blood_pressurediastolic.data_type = "diastolic";
                blood_pressurediastolic.data_unit = Unitdiastolic;
                blood_pressurediastolic.data_value = Math.Round(Convert.ToDouble(obj["dia"]),2);
                blood_pressurepulse.data_type = "pulse";
                blood_pressurepulse.data_unit = Unitpulse;
                blood_pressurepulse.data_value = Math.Round(Convert.ToDouble(obj["pul"]),2);
                
                string jsonDataSys = JsonConvert.SerializeObject(blood_pressuresystolic);
                string jsonDataDia = JsonConvert.SerializeObject(blood_pressurediastolic);
                string jsonDataPul = JsonConvert.SerializeObject(blood_pressurepulse);
                string insertvalues = "('" + jsonDataSys + "'),('" + jsonDataDia + "'),('" + jsonDataPul + "')";
                StagingTableInsertJson(stagingInsert + insertvalues, ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private static bool ProcessBloodGlucoseData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Fasting", out string Unitglucose);
                if (dev.data == null)
                    return false;
                JObject obj = JObject.Parse(dev.data.ToString());
                StagingInput blood_glucose = new StagingInput();
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                blood_glucose.reading_id = readingIdWithPrefix;
                blood_glucose.device_id = dev.deviceId;
                blood_glucose.device_model = dev.modelNumber;
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                var timezone= Convert.ToInt16(obj["tz_tz"]);
                blood_glucose.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //blood_glucose.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_glucose.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');

                blood_glucose.reading_type = "blood_glucose";
                blood_glucose.battery = 0;
                var time = obj["ts_tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    blood_glucose.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    blood_glucose.time_zone_offset = null;
                }
                blood_glucose.irregular = false;
                bool isUnitmmol = obj["unit"].ToString() == "1";
                bool isUnitmgdl = obj["unit"].ToString() == "2";
                blood_glucose.data_unit = Unitglucose;
                if (isUnitmmol)
                {
                    blood_glucose.data_value = Math.Round(Convert.ToDouble(obj["data"]) * 18,2);
                }
                else
                {
                    blood_glucose.data_value = Math.Round(Convert.ToDouble(obj["data"]),2);
                }
                bool isFasting = obj["meal"].ToString() == "1";
                bool isNonFasting = obj["meal"].ToString() == "2";
                bool isNone = obj["meal"].ToString() == "0";
                if (isFasting)
                {
                    blood_glucose.data_type = "Fasting";
                    blood_glucose.before_meal = true;
                    blood_glucose.event_flag = "0";
                }

                if (isNonFasting)
                {
                    blood_glucose.data_type = "Non-Fasting";
                    blood_glucose.before_meal = false;
                    blood_glucose.event_flag = "1";
                }
                if (isNone)
                {
                    var mealInfo = ProcessMealZero(dateTime, Convert.ToInt32(obj["ts_tz"]));
                    blood_glucose.data_type = mealInfo.data_type;
                    blood_glucose.before_meal = mealInfo.before_meal;
                    blood_glucose.event_flag = mealInfo.event_flag;
                }
                string jsonData = JsonConvert.SerializeObject(blood_glucose);
                StagingTableInsertJson(stagingInsert + "('" + jsonData + "')", ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        public static StagingInput ProcessMealZero(DateTime utcDateTime,int zone=0)
        {
            try
            {
                DateTime shifted = utcDateTime.AddHours(zone);
                StagingInput bg = new StagingInput();
                var t = shifted.TimeOfDay;
                bool val = t >= TimeSpan.FromHours(bg_fastingStartTime) && t < TimeSpan.FromHours(bg_fastingEndTime);
                if (val)
                {
                    bg.data_type = "Fasting";
                    bg.before_meal = true;
                    bg.event_flag = "0";
                }
                else
                {
                    bg.data_type = "Non-Fasting";
                    bg.before_meal = false;
                    bg.event_flag = "1";
                }
                return bg;
            }
            catch (Exception)
            {

                throw;
            }
           
        }
        private static bool ProcessWeightData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Weight", out string Unitweight);
                if (dev.data == null)
                    return false;
                JObject obj = JObject.Parse(dev.data.ToString());
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                StagingInput weight = new StagingInput();
                weight.reading_id = readingIdWithPrefix;
                weight.device_id = dev.deviceId;
                weight.device_model = dev.modelNumber;
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                weight.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //weight.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                weight.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                weight.reading_type = "weight";
                if (obj.TryGetValue("bat", out var batToken) && batToken != null && int.TryParse(batToken.ToString(), out int battery))
                {
                    weight.battery = battery;
                }
                else
                {
                    weight.battery = 0;
                }

                var time = obj["tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    weight.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    weight.time_zone_offset = null;
                }
                weight.before_meal = false;
                weight.event_flag = null;
                weight.irregular = false;
                weight.data_type = "weight";
                weight.data_unit = Unitweight;
                if (Unitweight == "kg")
                {
                    weight.data_value = Math.Round(Convert.ToDouble(obj["wt"]) / 1000,1);
                }
                else if (Unitweight == "lbs")
                {
                    weight.data_value = Math.Round(Convert.ToDouble(obj["wt"]) * 0.00220462,1);
                }
                string jsonData = JsonConvert.SerializeObject(weight);
                StagingTableInsertJson(stagingInsert + "('" + jsonData + "')", ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private static bool ProcessOxygenData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Pulse", out string Unitpulse);
                vitalunits_dictionary.TryGetValue("Oxygen", out string Unitspo2);
                JObject objOxygen = JObject.Parse(dev.data.ToString());
                string timeStamp = objOxygen["time"]?.ToString();
                long unixSeconds = ParseTranstekTimeToUnixSecondsUtc(timeStamp);
                string readingIdWithPrefix = "TK" + objOxygen["imei"] + unixSeconds;
                StagingInput pulseoximeter_oxygen = new StagingInput();
                StagingInput pulseoximeter_pulse = new StagingInput();
                pulseoximeter_oxygen.reading_id = readingIdWithPrefix;
                pulseoximeter_pulse.reading_id = readingIdWithPrefix;
                pulseoximeter_oxygen.device_id = dev.deviceId;
                pulseoximeter_pulse.device_id = dev.deviceId;
                pulseoximeter_oxygen.device_model = "BM1000";
                pulseoximeter_pulse.device_model = "BM1000";
                string timeString = objOxygen["time"].ToString();
                string dateTimePart = timeString.Substring(0, timeString.Length - 3);
                DateTime time = DateTime.ParseExact(dateTimePart, "yy/MM/dd,HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                string formattedTime = time.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                pulseoximeter_oxygen.date_recorded = formattedTime;
                pulseoximeter_pulse.date_recorded = formattedTime;
                //pulseoximeter_oxygen.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                //pulseoximeter_pulse.date_received = createdOn.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                var dateTimeRx = ConvertUnixTimestampToDateTime(dev.createdAt);
                pulseoximeter_oxygen.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                pulseoximeter_pulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':');
                pulseoximeter_oxygen.reading_type = "pulse_ox";
                pulseoximeter_pulse.reading_type = "pulse_ox";
                pulseoximeter_oxygen.battery = (int)objOxygen["battery"];
                pulseoximeter_pulse.battery = (int)objOxygen["battery"];
                string timeZone = objOxygen["time"].ToString();
                string timeZoneOffsetStr = timeZone.Substring(timeZone.Length - 3);
                if (int.TryParse(timeZoneOffsetStr, out int timeZoneOffsetInMinutes))
                {
                    int totalMinutes = timeZoneOffsetInMinutes * 15;
                    int timeZoneOffsetInHours = totalMinutes / 60;
                    int remainingMinutes = totalMinutes % 60;
                    pulseoximeter_oxygen.time_zone_offset = timeZoneOffsetInHours.ToString();
                    pulseoximeter_pulse.time_zone_offset = timeZoneOffsetInHours.ToString();
                }
                else
                {
                    pulseoximeter_oxygen.time_zone_offset = null;
                    pulseoximeter_pulse.time_zone_offset = null;
                }
                pulseoximeter_oxygen.before_meal = false;
                pulseoximeter_pulse.before_meal = false;
                pulseoximeter_oxygen.event_flag = null;
                pulseoximeter_pulse.event_flag = null;
                pulseoximeter_oxygen.irregular = false;
                pulseoximeter_pulse.irregular = false;
                pulseoximeter_oxygen.data_type = "Oxygen";
                pulseoximeter_oxygen.data_unit = Unitspo2;
                pulseoximeter_oxygen.data_value = Math.Round(Convert.ToDouble(objOxygen["spo2"]),2);
                pulseoximeter_pulse.data_type = "Pulse";
                pulseoximeter_pulse.data_unit = Unitpulse;
                pulseoximeter_pulse.data_value = Math.Round(Convert.ToDouble(objOxygen["pr"]),2);
                string jsonDataOx = JsonConvert.SerializeObject(pulseoximeter_oxygen);
                string jsonDataPulse = JsonConvert.SerializeObject(pulseoximeter_pulse);
                string insertvalues = "('" + jsonDataOx + "'),('" + jsonDataPulse + "')";
                StagingTableInsertJson(stagingInsert + insertvalues, ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private static long ParseTranstekTimeToUnixSecondsUtc(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            timeString = timeString.Trim();
            if (timeString.Length < 3)
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Last 3 chars are signed offset units of 15 minutes (e.g. "+04" or "-02")
            string offsetRaw = timeString.Substring(timeString.Length - 3);
            string datePart = timeString.Substring(0, timeString.Length - 3).Trim();

            // Parse date part (expected "yy/MM/dd,HH:mm:ss")
            if (!DateTime.TryParseExact(datePart, "yy/MM/dd,HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedLocal))
            {
                // fallback to loose parse
                if (!DateTime.TryParse(datePart, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedLocal))
                    return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            if (!int.TryParse(offsetRaw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int offsetUnits))
                offsetUnits = 0;

            // offsetUnits * 15 minutes => TimeSpan offset
            var offset = TimeSpan.FromMinutes(offsetUnits * 15);
            var dtoWithOffset = new DateTimeOffset(parsedLocal, offset);

            return dtoWithOffset.ToUniversalTime().ToUnixTimeSeconds();
        }
        private static bool StagingTableInsertJson(string jsonData, string ConnectionString)
        {
            bool ret;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsJsonStg_FromTranstekJsonStg", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Json", jsonData);
                    command.Parameters.AddWithValue("@CreatedBy", DateTime.Now.ToUniversalTime());
                    con.Open();
                    command.ExecuteScalar();
                    con.Close();
                    Console.WriteLine($"Inserted reading for deviceId: {jsonData}, time:{DateTime.Now.ToUniversalTime()}");
                }
                ret = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

    }
}
