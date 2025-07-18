using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace azuretranstekwebjob
{
    internal class Program
    {
        static string CONN_STRING = string.Empty;
        static ConcurrentDictionary<string, DeviceIDs> deviceid_dictionary = new ConcurrentDictionary<string, DeviceIDs>();
        static ConcurrentDictionary<string, string> vitalunits_dictionary = new ConcurrentDictionary<string, string>();
        static string readingid = string.Empty;
        static string acess_key = string.Empty;
        static async Task Main(string[] args)
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

                    string sdata = GetJsonFromStagingTable(val.Deviceid, newstart);
                    if (sdata == null)
                        continue;
                    JObject parsedJson = JObject.Parse(sdata);
                    JArray readingsArray = parsedJson["readings"] as JArray;
                    string deviceType = IsValidlifesenseDevice(val.Deviceid);
                    if (string.IsNullOrEmpty(deviceType))
                    {
                        Console.WriteLine("Device type not valid or not found.");
                        continue;
                    }
                    List<DateTime> receivedTimes = new List<DateTime>();
                    if (readingsArray.Count > 0)
                    {
                        foreach (var readingItem in readingsArray)
                        {
                            try
                            {
                                // Each item is a JSON-encoded string, so parse it again
                                string readingJson = readingItem.ToString();
                                if (readingJson == null) continue;
                                TranstekDeviceTelemetry telemetry = JsonConvert.DeserializeObject<TranstekDeviceTelemetry>(readingJson);

                                if (telemetry != null)
                                {

                                    if (telemetry.messageType == "telemetry")
                                    {

                                        if (telemetry.data == null || telemetry.data.ToString() == "{}")
                                        {
                                            continue;
                                        }
                                        bool insertSuccess = StagingTableQueueInsert(telemetry, deviceType);

                                        if (insertSuccess)
                                        {
                                            Console.WriteLine($"Successfully inserted reading for deviceId: {telemetry}");
                                            // Track createdAt timestamp
                                            DateTime receivedTime = ConvertUnixTimestampToDateTime(telemetry.createdAt);
                                            receivedTimes.Add(receivedTime);
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Failed to insert reading for deviceId: {telemetry.deviceId}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Skipping non-telemetry messageType: {telemetry.messageType}");
                                    }
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
                        // After processing all readings, update last batch if we inserted any
                        if (receivedTimes.Any())
                        {
                            DateTime maxReceivedTime = receivedTimes.Max();
                            UpdateAppJobLastBatch(val.Deviceid, maxReceivedTime);
                        }
                        else
                        {
                            DateTime newenddate = newstart.AddDays(2);
                            UpdateAppJobLastBatch(val.Deviceid, newenddate);
                        }
                    }
                    else
                    {
                        DateTime newenddate = newstart.AddDays(2);
                        UpdateAppJobLastBatch(val.Deviceid, newenddate);
                    }
                }
                Console.WriteLine("TimerApiCallback end");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception before StagingTableQueueInsert:" + ex);
            }
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
        private static string GetJsonFromStagingTable(string deviceId, DateTime startDate)
        {
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
                            jsonReadings.Add(reader.GetString(0)); // raw JSON string
                        }
                    }
                }
                return JsonConvert.SerializeObject(jsonReadings);


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

            if (deviceType == "Blood Pressure Monitor ")
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
                blood_pressuresystolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurediastolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurepulse.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_pressuresystolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurediastolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurepulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
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
                blood_pressuresystolic.data_value = Convert.ToDouble(obj["sys"]);
                blood_pressurediastolic.data_type = "diastolic";
                blood_pressurediastolic.data_unit = Unitdiastolic;
                blood_pressurediastolic.data_value = Convert.ToDouble(obj["dia"]);
                blood_pressurepulse.data_type = "pulse";
                blood_pressurepulse.data_unit = Unitpulse;
                blood_pressurepulse.data_value = Convert.ToDouble(obj["pul"]);
                
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
                blood_glucose.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_glucose.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
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
                    blood_glucose.data_value = Convert.ToDouble(obj["data"]) * 18;
                }
                else
                {
                    blood_glucose.data_value = Convert.ToDouble(obj["data"]);
                }
                bool isFasting = obj["meal"].ToString() == "1";
                bool isNonFasting = obj["meal"].ToString() == "2";
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
                weight.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                weight.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
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
                    weight.data_value = Convert.ToDouble(obj["wt"]) / 1000;
                }
                else if (Unitweight == "lbs")
                {
                    weight.data_value = Convert.ToDouble(obj["wt"]) * 0.00220462;
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
                JObject objOxygen = JObject.Parse(dev.deviceData.ToString());
                string readingIdWithPrefix = "TK" + objOxygen["imei"] + objOxygen["ts"];
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
                string formattedTime = time.ToString("yyyy-MM-dd HH:mm:ss");
                pulseoximeter_oxygen.date_recorded = formattedTime;
                pulseoximeter_pulse.date_recorded = formattedTime;
                var dateTimeRx = ConvertUnixTimestampToDateTime(dev.createdAt);
                pulseoximeter_oxygen.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                pulseoximeter_pulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
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
                pulseoximeter_oxygen.data_value = Convert.ToDouble(objOxygen["spo2"]);
                pulseoximeter_pulse.data_type = "Pulse";
                pulseoximeter_pulse.data_unit = Unitpulse;
                pulseoximeter_pulse.data_value = Convert.ToDouble(objOxygen["pr"]);
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
