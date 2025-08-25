using iglucosedata;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Data;
//cron continuous
class Program
{
    static ConcurrentDictionary<string, DeviceIDs> deviceid_dictionary = new ConcurrentDictionary<string, DeviceIDs>();
    static ConcurrentDictionary<string, string> vitalunits_dictionary = new ConcurrentDictionary<string, string>();     
    static string readingid = string.Empty;
    static string acess_key = string.Empty;
    static string CONN_STRING = string.Empty;
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
        if(CONN_STRING == null)
        {
            Console.WriteLine("Connection string is null.");
            return;
        }

        List<SystemConfigInfo> igc = Data.GetSystemConfig(CONN_STRING, "iGlucose", "User");

        SystemConfigInfo? igckey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
        acess_key = igckey.Value;
        Thread.Sleep(10000);
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
                command.Parameters.AddWithValue("@deviceVendorName", "iGlucose");
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
                Console.WriteLine("DeviceId: " + val.Deviceid + "; Lasted Recorded :" + lastRecodDate);
                DateTime newstart = lastRecodDate.AddSeconds(2);
                string sdata = Data.CallRestMethod("HTTPS://api.iglucose.com/readings/", newstart, val.Deviceid, acess_key);
                dynamic data = JObject.Parse(sdata);
                JArray array = data.readings;
                if (array.Count > 0)
                {
                    StagingTableQueueInsert(array, val.Deviceid);
                }
                else
                {
                    DateTime newenddate = newstart.AddDays(2);
                    if (newenddate <= DateTime.Now)
                    {
                        using (SqlConnection connection = new SqlConnection(CONN_STRING))
                        {
                            SqlCommand command = new SqlCommand("usp_AppJobLastBatch", connection);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@DeviceSerialNo", val.Deviceid);
                            command.Parameters.AddWithValue("@DeviceVendorName", "iGlucose");
                            command.Parameters.AddWithValue("@DateRecorded", newstart.AddDays(2));
                            SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                            returnParameter.Direction = ParameterDirection.ReturnValue;
                            connection.Open();
                            command.ExecuteNonQuery();
                            int id = (int)returnParameter.Value;
                            connection.Close();
                            if (id.Equals(0))
                            {

                            }
                            else
                            {
                                Console.WriteLine(val.Deviceid + " Lastupdated:" + newstart.AddDays(2));

                            }

                        }
                    }

                }
            }
            Console.WriteLine("TimerApiCallback end");
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception before StagingTableQueueInsert:" + ex);
        }
    }
    private static void StagingTableQueueInsert(JArray array, string deviceId)
    {

        try
        {
            string reading_type = string.Empty;
            foreach (JObject obj in array)
            {
                reading_type = obj["reading_type"].ToString();
                break;
            }
            if (!String.IsNullOrEmpty(reading_type))
            {
                dynamic ig;
                if (reading_type == "blood_glucose")
                { ig = array.ToObject<List<iGlucoseBG>>(); }
                else if (reading_type == "blood_pressure")
                { ig = array.ToObject<List<iGlucoseBP>>(); }
                else if (reading_type == "weight")
                { ig = array.ToObject<List<iGlucoseW>>(); }
                else if (reading_type == "pulse_ox")
                { ig = array.ToObject<List<iGlucosePulse>>(); }

                else
                {
                    ig = null;
                }
                List<DateTime> ReceivedTimes = new List<DateTime>();
                for (int j = 0; j < ig.Count; j++)
                {
                    if (deviceId == ig[j].device_id.ToString())
                    {
                        ReceivedTimes.Add(Convert.ToDateTime(ig[j].date_received));
                        Console.WriteLine("deviceid:" + ig[j].device_id);
                        Console.WriteLine("date_recorded:" + ig[j].date_recorded);
                        if (reading_type == "blood_glucose")
                        {
                            string Unitglucose;
                            vitalunits_dictionary.TryGetValue("Fasting", out Unitglucose);
                            DatabaseInput blood_glucose = new DatabaseInput();
                            blood_glucose.reading_id = ig[j].reading_id;
                            blood_glucose.device_id = ig[j].device_id;
                            blood_glucose.device_model = ig[j].device_model;
                            blood_glucose.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_glucose.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_glucose.reading_type = ig[j].reading_type;
                            blood_glucose.battery = ig[j].battery;
                            var time = ig[j].time_zone_offset;
                            if (time != null)
                            {
                                blood_glucose.time_zone_offset = time.ToString();
                            }
                            else
                            {
                                blood_glucose.time_zone_offset = null;
                            }
                            blood_glucose.data_unit = Unitglucose;
                            if (Unitglucose == "mgdl")
                            {
                                blood_glucose.data_value = ig[j].blood_glucose_mgdl;
                            }
                            else if (Unitglucose == "mmol")
                            {
                                blood_glucose.data_value = ig[j].blood_glucose_mmol;
                            }
                            blood_glucose.before_meal = ig[j].before_meal;
                            if (ig[j].before_meal == true)
                            {
                                blood_glucose.data_type = "Fasting";
                            }
                            else
                            {
                                blood_glucose.data_type = "Non-Fasting";
                            }
                            blood_glucose.event_flag = ig[j].event_flag;
                            blood_glucose.irregular = ig[j].irregular;
                            TimerStagingTableInsert(blood_glucose);
                            Console.WriteLine("Added to queue:" + blood_glucose.device_id);

                        }
                        else if (reading_type == "blood_pressure")
                        {
                            string Unitsystolic;
                            vitalunits_dictionary.TryGetValue("systolic", out Unitsystolic);
                            string Unitdiastolic;
                            vitalunits_dictionary.TryGetValue("diastolic", out Unitdiastolic);
                            string Unitpulse;
                            vitalunits_dictionary.TryGetValue("pulse", out Unitpulse);
                            DatabaseInput blood_pressuresystolic = new DatabaseInput();
                            DatabaseInput blood_pressurediastolic = new DatabaseInput();
                            DatabaseInput blood_pressurepulse = new DatabaseInput();
                            blood_pressuresystolic.reading_id = ig[j].reading_id;
                            blood_pressurediastolic.reading_id = ig[j].reading_id;
                            blood_pressurepulse.reading_id = ig[j].reading_id;
                            blood_pressuresystolic.device_id = ig[j].device_id;
                            blood_pressurediastolic.device_id = ig[j].device_id;
                            blood_pressurepulse.device_id = ig[j].device_id;
                            blood_pressuresystolic.device_model = ig[j].device_model;
                            blood_pressurediastolic.device_model = ig[j].device_model;
                            blood_pressurepulse.device_model = ig[j].device_model;
                            blood_pressuresystolic.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressurediastolic.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressurepulse.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressuresystolic.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressurediastolic.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressurepulse.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            blood_pressuresystolic.reading_type = ig[j].reading_type;
                            blood_pressurediastolic.reading_type = ig[j].reading_type;
                            blood_pressurepulse.reading_type = ig[j].reading_type;
                            blood_pressuresystolic.battery = 0;
                            blood_pressurediastolic.battery = 0;
                            blood_pressurepulse.battery = 0;
                            var time = ig[j].time_zone_offset;
                            if (time != null)
                            {
                                blood_pressuresystolic.time_zone_offset = time.ToString();
                                blood_pressurediastolic.time_zone_offset = time.ToString();
                                blood_pressurepulse.time_zone_offset = time.ToString();
                            }
                            else
                            {
                                blood_pressuresystolic.time_zone_offset = null;
                                blood_pressurediastolic.time_zone_offset = null;
                                blood_pressurepulse.time_zone_offset = null;
                            }

                            blood_pressuresystolic.before_meal = ig[j].before_meal;
                            blood_pressurediastolic.before_meal = ig[j].before_meal;
                            blood_pressurepulse.before_meal = ig[j].before_meal;
                            blood_pressuresystolic.event_flag = ig[j].event_flag;
                            blood_pressurediastolic.event_flag = ig[j].event_flag;
                            blood_pressurepulse.event_flag = ig[j].event_flag;
                            blood_pressuresystolic.irregular = ig[j].irregular;
                            blood_pressurediastolic.irregular = ig[j].irregular;
                            blood_pressurepulse.irregular = ig[j].irregular;
                            blood_pressuresystolic.data_type = "systolic";
                            blood_pressuresystolic.data_unit = Unitsystolic;
                            blood_pressuresystolic.data_value = ig[j].systolic_mmhg;
                            TimerStagingTableInsert(blood_pressuresystolic);
                            blood_pressurediastolic.data_type = "diastolic";
                            blood_pressurediastolic.data_unit = Unitdiastolic;
                            blood_pressurediastolic.data_value = ig[j].diastolic_mmhg;
                            TimerStagingTableInsert(blood_pressurediastolic);
                            Console.WriteLine("Added to queue:" + blood_pressurediastolic.device_id);
                            blood_pressurepulse.data_type = "pulse";
                            blood_pressurepulse.data_unit = Unitpulse;
                            blood_pressurepulse.data_value = ig[j].pulse_bpm;
                            TimerStagingTableInsert(blood_pressurepulse);
                            Console.WriteLine("Added to queue:" + blood_pressurepulse.device_id);
                        }
                        else if (reading_type == "weight")
                        {
                            string Unitweight;
                            vitalunits_dictionary.TryGetValue("Weight", out Unitweight);
                            DatabaseInput weight = new DatabaseInput();
                            weight.reading_id = ig[j].reading_id;
                            weight.device_id = ig[j].device_id;
                            weight.device_model = ig[j].device_model;
                            weight.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            weight.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            weight.reading_type = ig[j].reading_type;
                            weight.battery = 0;
                            var time = ig[j].time_zone_offset;
                            if (time != null)
                            {
                                weight.time_zone_offset = time.ToString();
                            }
                            else
                            {
                                weight.time_zone_offset = null;
                            }
                            weight.data_type = "weight";
                            weight.data_unit = Unitweight;
                            if (Unitweight == "kg")
                            {
                                weight.data_value = ig[j].weight_kg;
                            }
                            else if (Unitweight == "lbs")
                            {
                                weight.data_value = ig[j].weight_lbs;
                            }
                            weight.before_meal = ig[j].before_meal;
                            weight.event_flag = ig[j].event_flag;
                            weight.irregular = ig[j].irregular;
                            TimerStagingTableInsert(weight);

                        }
                        else if (reading_type == "pulse_ox")
                        {
                            string Unitpulse;
                            vitalunits_dictionary.TryGetValue("Pulse", out Unitpulse);
                            string Unitspo2;
                            vitalunits_dictionary.TryGetValue("Oxygen", out Unitspo2);
                            DatabaseInput pulse_ox_spo2 = new DatabaseInput();
                            DatabaseInput pulse_ox_pulse = new DatabaseInput();
                            pulse_ox_spo2.reading_id = ig[j].reading_id;
                            pulse_ox_pulse.reading_id = ig[j].reading_id;
                            pulse_ox_spo2.device_id = ig[j].device_id;
                            pulse_ox_pulse.device_id = ig[j].device_id;
                            pulse_ox_spo2.device_model = ig[j].device_model;
                            pulse_ox_pulse.device_model = ig[j].device_model;
                            pulse_ox_spo2.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            pulse_ox_pulse.date_recorded = ig[j].date_recorded.ToString("yyyy-MM-dd HH:mm:ss");
                            pulse_ox_spo2.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            pulse_ox_pulse.date_received = ig[j].date_received.ToString("yyyy-MM-dd HH:mm:ss");
                            pulse_ox_spo2.reading_type = ig[j].reading_type;
                            pulse_ox_pulse.reading_type = ig[j].reading_type;
                            pulse_ox_spo2.battery = 0;
                            pulse_ox_pulse.battery = 0;
                            var time = ig[j].time_zone_offset;
                            if (time != null)
                            {
                                pulse_ox_spo2.time_zone_offset = time.ToString();
                            }
                            else
                            {
                                pulse_ox_spo2.time_zone_offset = null;
                            }
                            var time1 = ig[j].time_zone_offset;
                            if (time != null)
                            {
                                pulse_ox_pulse.time_zone_offset = time1.ToString();
                            }
                            else
                            {
                                pulse_ox_pulse.time_zone_offset = null;
                            }
                            pulse_ox_spo2.data_type = "Oxygen";
                            pulse_ox_pulse.data_type = "Pulse";
                            pulse_ox_spo2.data_unit = Unitspo2;
                            pulse_ox_pulse.data_unit = Unitpulse;
                            pulse_ox_spo2.data_value = ig[j].spo2;
                            pulse_ox_pulse.data_value = ig[j].pulse_bpm;
                            pulse_ox_spo2.before_meal = ig[j].before_meal;
                            pulse_ox_pulse.before_meal = ig[j].before_meal;
                            pulse_ox_spo2.event_flag = ig[j].event_flag;
                            pulse_ox_pulse.event_flag = ig[j].event_flag;
                            pulse_ox_spo2.irregular = ig[j].irregular;
                            pulse_ox_pulse.irregular = ig[j].irregular;
                            TimerStagingTableInsert(pulse_ox_spo2);
                            TimerStagingTableInsert(pulse_ox_pulse);
                        }
                        if (j == ig.Count - 1)
                        {
                            DateTime MaxDate = ReceivedTimes.Max();
                            using (SqlConnection connection = new SqlConnection(CONN_STRING))
                            {
                                SqlCommand command = new SqlCommand("usp_AppJobLastBatch", connection);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@DeviceSerialNo", ig[j].device_id);
                                command.Parameters.AddWithValue("@DeviceVendorName", "iGlucose");
                                command.Parameters.AddWithValue("@DateRecorded", MaxDate);
                                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                                returnParameter.Direction = ParameterDirection.ReturnValue;
                                connection.Open();
                                command.ExecuteNonQuery();
                                int id = (int)returnParameter.Value;
                                connection.Close();
                                if (id.Equals(0))
                                {

                                }
                                else
                                {
                                    Console.WriteLine(ig[j].device_id + " Lastupdated:" + ig[j].date_received);

                                }

                            }
                        }

                    }
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception: after StagingTableQueueInsert" + ex);
        }
    }
    private static void TimerStagingTableInsert(DatabaseInput reading)
    {

        try
        {
            string jsonData = JsonConvert.SerializeObject(reading);
            
            Console.WriteLine("JsonStg Insert begin: " + jsonData);
            using (SqlConnection connection = new SqlConnection(CONN_STRING))
            {
                SqlCommand command = new SqlCommand("usp_InsJsonStg", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@json", jsonData);
                command.Parameters.AddWithValue("@CreatedBy", DateTime.Now.ToUniversalTime());
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception:TimerStagingTableInsert" + ex);
        }
        Console.WriteLine("TimerStagingTableInsert end");
    }
}
