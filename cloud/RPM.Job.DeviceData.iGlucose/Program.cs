using iglucosedata;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Data;

class Program
{
    static ConcurrentDictionary<string, DeviceIDs> deviceid_dictionary = new ConcurrentDictionary<string, DeviceIDs>();
    static ConcurrentDictionary<string, string> vitalunits_dictionary = new ConcurrentDictionary<string, string>();
    static string readingid = string.Empty;
    static string? acess_key = string.Empty;
    static string? CONN_STRING = string.Empty;
    static void Main(string[] args)
    {
        // Set up configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables() // Allows overriding via Azure App Settings
            .Build();

        // Access a specific config value
        string? connStr = config["RPM:ConnectionString"];
        Console.WriteLine($"RPM Connection String: {connStr}");
        // Optional: bind strongly-typed object
        var rpmSettings = config.GetSection("RPM").Get<RpmSettings>();
        Console.WriteLine($"RPM.ConnectionString (typed): {rpmSettings?.ConnectionString}");
        CONN_STRING = rpmSettings?.ConnectionString;
        Console.WriteLine("WebJob started...");
        if (CONN_STRING == null)
        {
            Console.WriteLine("Connection string is null.");
            return;
        }

        List<SystemConfigInfo> igc = Data.GetSystemConfig(CONN_STRING, "iGlucose", "User");

        SystemConfigInfo? igckey = igc.Find(x => x.Name.Equals("ApiKey_iGlucose"));
        acess_key = igckey?.Value;
        if (acess_key == null)
        {
            Console.WriteLine("access key is null.");
            return;
        }
        Thread.Sleep(10000);
        try
        {
            TimerDeviceIdCallback();
            Thread.Sleep(1000);
            TimerApiCallback();
            Thread.Sleep(20000);
        }
        catch (Exception)
        {
        }
    }
    private static void TimerDeviceIdCallback()
    {
        try
        {
            Console.WriteLine("Reading DeviceId and Vitals");

            var latestDevices = LoadDeviceIds();
            UpdateDeviceDictionary(latestDevices);

            var vitalUnits = LoadVitalUnits();
            UpdateVitalUnitsDictionary(vitalUnits);

            Console.WriteLine("Reading DeviceId and Vitals end");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in TimerDeviceIdCallback: " + ex);
        }
    }
    private static Dictionary<string, DeviceIDs> LoadDeviceIds()
    {
        var result = new Dictionary<string, DeviceIDs>();

        using var connection = new SqlConnection(CONN_STRING);
        using var command = new SqlCommand("usp_GetDeviceIds", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@deviceVendorName", "iGlucose");

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string? deviceId = reader["DeviceSerialNo"].ToString();
            DateTime activatedDate = (DateTime)reader["DeviceActivatedDateTime"];
            if (!string.IsNullOrEmpty(deviceId))
            {
                result[deviceId] = new DeviceIDs(deviceId, activatedDate);
            }
        }
        return result;
    }

    private static void UpdateDeviceDictionary(Dictionary<string, DeviceIDs> latestDevices)
    {
        // Add or update new devices
        foreach (var kvp in latestDevices)
        {
            deviceid_dictionary.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
        }
        // Remove old devices not present anymore
        foreach (var existing in deviceid_dictionary.Keys)
        {
            if (!latestDevices.ContainsKey(existing))
            {
                deviceid_dictionary.TryRemove(existing, out _);
            }
        }
    }

    private static Dictionary<string, string> LoadVitalUnits()
    {
        var result = new Dictionary<string, string>();

        using var connection = new SqlConnection(CONN_STRING);
        using var command = new SqlCommand("usp_GetVitalUnits", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string? measureName = reader["MeasureName"].ToString();
            string unit = reader["UnitName"].ToString()??"";
            if (!string.IsNullOrEmpty(measureName))
            {
                result[measureName] = unit;
            }
        }
        return result;
    }

    private static void UpdateVitalUnitsDictionary(Dictionary<string, string> latestUnits)
    {
        foreach (var kvp in latestUnits)
        {
            vitalunits_dictionary.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => kvp.Value);
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
                TimeSpan difference = (lastRecodDate - DateTime.UtcNow).Duration();
                if (difference.TotalMinutes > 30)
                {
                    GetDeviceDataFromAPI(lastRecodDate, val.Deviceid);
                }
            }
            Console.WriteLine("TimerApiCallback end");
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception before StagingTableQueueInsert:" + ex);
        }
    }
    private static void GetDeviceDataFromAPI(DateTime lastRecodDate,String Deviceid)
    {
        DateTime newstart = lastRecodDate.AddSeconds(2);
        string sdata = Data.CallRestMethod("HTTPS://api.iglucose.com/readings/", newstart, Deviceid, acess_key);
        dynamic data = JObject.Parse(sdata);
        JArray array = data.readings;
        if (array.Count > 0)
        {
            StagingTableQueueInsert(array, Deviceid);
        }
        else
        {
            DateTime newenddate = newstart.AddDays(2);
            if (newenddate <= DateTime.UtcNow)
            {
                UpdateLastBatch(Deviceid, newenddate);
            }
        }
    }
    private static void StagingTableQueueInsert(JArray array, string deviceId)
    {
        try
        {
            if (array == null || array.Count == 0) return;

            string? readingType = array.First?["reading_type"]?.ToString();
            if (string.IsNullOrEmpty(readingType)) return;

            dynamic? value = readingType switch
            {
                "blood_glucose" => array.ToObject<List<iGlucoseBG>>(),
                "blood_pressure" => array.ToObject<List<iGlucoseBP>>(),
                "weight" => array.ToObject<List<iGlucoseW>>(),
                "pulse_ox" => array.ToObject<List<iGlucosePulse>>(),
                _ => null
            };
            dynamic? readings = value;

            if (readings == null) return;

            List<DateTime> receivedTimes = new();

            foreach (var reading in readings)
            {
                if (deviceId != reading.device_id.ToString()) continue;

                receivedTimes.Add(Convert.ToDateTime(reading.date_received));
                Console.WriteLine($"deviceid: {reading.device_id}, date_recorded: {reading.date_recorded}");

                switch (readingType)
                {
                    case "blood_glucose":
                        HandleBloodGlucose(reading);
                        break;
                    case "blood_pressure":
                        HandleBloodPressure(reading);
                        break;
                    case "weight":
                        HandleWeight(reading);
                        break;
                    case "pulse_ox":
                        HandlePulseOx(reading);
                        break;
                }
            }

            if (receivedTimes.Count > 0)
            {
                DateTime maxDate = receivedTimes.Max();
                UpdateLastBatch(deviceId, maxDate);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in StagingTableQueueInsert: " + ex);
        }
    }
    private static void HandleBloodGlucose(iGlucoseBG reading)
    {
        vitalunits_dictionary.TryGetValue("Fasting", out string? unit);

        var input = new DatabaseInput
        {
            reading_id = reading.reading_id,
            device_id = reading.device_id,
            device_model = reading.device_model,
            date_recorded = reading.date_recorded.ToString("yyyy-MM-dd HH:mm:ss"),
            date_received = reading.date_received.ToString("yyyy-MM-dd HH:mm:ss"),
            reading_type = reading.reading_type,
            battery = reading.battery,
            time_zone_offset = reading.time_zone_offset.ToString(),
            data_unit = unit??"",
            data_value = unit == "mgdl" ? reading.blood_glucose_mgdl : reading.blood_glucose_mmol,
            before_meal = reading.before_meal,
            data_type = reading.before_meal ? "Fasting" : "Non-Fasting",
            event_flag = reading.event_flag,
            irregular = reading.irregular
        };

        StagingTableInsertProc(input);
        Console.WriteLine("Added to queue: " + input.device_id);
    }

    private static void HandleBloodPressure(iGlucoseBP reading)
    {
        vitalunits_dictionary.TryGetValue("systolic", out string? unitSystolic);
        vitalunits_dictionary.TryGetValue("diastolic", out string? unitDiastolic);
        vitalunits_dictionary.TryGetValue("pulse", out string? unitPulse);

        InsertBloodPressureRecord(reading, "systolic", unitSystolic, reading.systolic_mmhg);
        InsertBloodPressureRecord(reading, "diastolic", unitDiastolic, reading.diastolic_mmhg);
        InsertBloodPressureRecord(reading, "pulse", unitPulse, reading.pulse_bpm);
    }

    private static void InsertBloodPressureRecord(iGlucoseBP reading, string type, string? unit, double value)
    {
        var input = new DatabaseInput
        {
            reading_id = reading.reading_id,
            device_id = reading.device_id,
            device_model = reading.device_model,
            date_recorded = reading.date_recorded.ToString("yyyy-MM-dd HH:mm:ss"),
            date_received = reading.date_received.ToString("yyyy-MM-dd HH:mm:ss"),
            reading_type = reading.reading_type,
            battery = 0,
            time_zone_offset = reading.time_zone_offset.ToString(),
            data_type = type,
            data_unit = unit ?? "",
            data_value = value,
            before_meal = reading.before_meal,
            event_flag = reading.event_flag,
            irregular = reading.irregular
        };

        StagingTableInsertProc(input);
        Console.WriteLine("Added to queue: " + input.device_id);
    }

    private static void HandleWeight(iGlucoseW reading)
    {
        vitalunits_dictionary.TryGetValue("Weight", out string? unit);

        var input = new DatabaseInput
        {
            reading_id = reading.reading_id,
            device_id = reading.device_id,
            device_model = reading.device_model,
            date_recorded = reading.date_recorded.ToString("yyyy-MM-dd HH:mm:ss"),
            date_received = reading.date_received.ToString("yyyy-MM-dd HH:mm:ss"),
            reading_type = reading.reading_type,
            battery = 0,
            time_zone_offset = reading.time_zone_offset.ToString(),
            data_type = "weight",
            data_unit = unit ?? "",
            data_value = unit == "kg" ? reading.weight_kg : reading.weight_lbs,
            before_meal = reading.before_meal,
            event_flag = reading.event_flag,
            irregular = reading.irregular
        };

        StagingTableInsertProc(input);
    }

    private static void HandlePulseOx(iGlucosePulse reading)
    {
        vitalunits_dictionary.TryGetValue("Pulse", out string? unitPulse);
        vitalunits_dictionary.TryGetValue("Oxygen", out string? unitOxygen);

        InsertPulseOxRecord(reading, "Oxygen", unitOxygen, reading.spo2);
        InsertPulseOxRecord(reading, "Pulse", unitPulse, reading.pulse_bpm);
    }

    private static void InsertPulseOxRecord(iGlucosePulse reading, string type, string? unit, double value)
    {
        var input = new DatabaseInput
        {
            reading_id = reading.reading_id,
            device_id = reading.device_id,
            device_model = reading.device_model,
            date_recorded = reading.date_recorded.ToString("yyyy-MM-dd HH:mm:ss"),
            date_received = reading.date_received.ToString("yyyy-MM-dd HH:mm:ss"),
            reading_type = reading.reading_type,
            battery = 0,
            time_zone_offset = reading.time_zone_offset.ToString(),
            data_type = type,
            data_unit = unit ?? "",
            data_value = value,
            before_meal = reading.before_meal,
            event_flag = reading.event_flag,
            irregular = reading.irregular
        };

        StagingTableInsertProc(input);
    }
    private static void UpdateLastBatch(string deviceId, DateTime maxDate)
    {
        using SqlConnection connection = new(CONN_STRING);
        using SqlCommand command = new("usp_AppJobLastBatch", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@DeviceSerialNo", deviceId);
        command.Parameters.AddWithValue("@DeviceVendorName", "iGlucose");
        command.Parameters.AddWithValue("@DateRecorded", maxDate);

        var returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
        returnParameter.Direction = ParameterDirection.ReturnValue;

        connection.Open();
        command.ExecuteNonQuery();

        if ((int)returnParameter.Value != 0)
        {
            Console.WriteLine($"{deviceId} Lastupdated: {maxDate}");
        }
    }

    private static void StagingTableInsertProc(DatabaseInput reading)
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
                command.Parameters.AddWithValue("@CreatedBy", DateTime.UtcNow);
                connection.Open();
                command.ExecuteScalar();
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception:StagingTableInsertProc" + ex);
        }
        Console.WriteLine("StagingTableInsertProc end");
    }
}
public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
