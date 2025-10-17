using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;

namespace RPM.Job.Simulator.iGlucose
{

    internal class ReadingService
    {
        private readonly string _connectionString;
       
        static ConcurrentDictionary<string, DeviceIDs> deviceid_dictionary = new ConcurrentDictionary<string, DeviceIDs>();
        public ReadingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        //Fetch the devices form the Device table with DeviceStatus = "Active" 
        public void getandProcessActiveDevices()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using var command = new SqlCommand("usp_GetDeviceIds_Simulator", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@deviceVendorName", "iGlucose");
                conn.Open();
                foreach (var key in deviceid_dictionary.Keys)
                {
                        deviceid_dictionary.TryRemove(key, out _);
                }
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string deviceSerialNo = reader["DeviceSerialNo"].ToString()!;
                        int deviceTypeId = Convert.ToInt32(reader["DeviceTypeId"]);
                        string deviceModel = reader["DeviceModel"].ToString()!;
                        DateTime activatedDate = (DateTime)reader["DeviceActivatedDateTime"];
                        if (!string.IsNullOrEmpty(deviceSerialNo))
                        {
                            deviceid_dictionary.AddOrUpdate(
                            deviceSerialNo,
                            key => new DeviceIDs(deviceSerialNo, deviceTypeId, deviceModel, activatedDate),
                            (key, oldValue) => new DeviceIDs(deviceSerialNo, deviceTypeId, deviceModel, activatedDate)
                            );
                            
                        }
                        
                    }
                }
             
                foreach (var device in deviceid_dictionary)
                {
                    Console.WriteLine($"Processing Device: {device.Value.DeviceSerialNo}, TypeId: {device.Value.DeviceTypeId}, Model: {device.Value.DeviceModel},Time:{device.Value.DeviceActivatedDateTime}");
                    GenerateAndInsertReading(conn, device.Value);
                }
            }
        }

        private void GenerateAndInsertReading(SqlConnection conn,  DeviceIDs device)
        {
           
            Random rndtime = new Random();
            DateTime baseDate = DateTime.UtcNow.Date.AddDays(-1);

            int randomHours = rndtime.Next(0, 24);
            int randomMinutes = rndtime.Next(0, 60);
            int randomSeconds = rndtime.Next(0, 60);

            DateTime nextDateTime = baseDate
                                    .AddHours(randomHours)
                                    .AddMinutes(randomMinutes)
                                    .AddSeconds(randomSeconds);
            if (nextDateTime>= DateTime.UtcNow)
            {
                return;
            }
            Random rnd = new Random();
            string vitalType;
            int? systolic = null, diastolic = null, pulse = null, glucose = null;
            decimal? weight = null;
            int? oxygen = null;

            bool? isFasting = null;

            //Time zone offset from system
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            string timeZoneOffset = $"{(offset.Hours >= 0 ? "+" : "-")}{offset.Hours:00}:{offset.Minutes:00}";

            switch (device.DeviceTypeId)
            {
                case 1: // Blood Pressure
                    vitalType = "blood_pressure";
                    systolic = rnd.Next(90, 150);
                    diastolic = rnd.Next(60, 100);
                    pulse = rnd.Next(60, 120);
                    break;

                case 2: // Blood Glucose
                    vitalType = "blood_glucose";
                    glucose = rnd.Next(70, 180);
                    isFasting = rnd.Next(0, 2) == 0;
                    break;

                case 3: // Weight
                    vitalType = "body_weight";
                    weight = rnd.Next(50, 120);
                    break;

                case 4: // Pulse Oximeter
                    vitalType = "pulse_oximeter";
                    oxygen = rnd.Next(90, 100);
                    pulse = rnd.Next(60, 120);
                    break;

                default:
                    Console.WriteLine($"Skipping unknown device type: {device.DeviceTypeId}");
                    return;
            }
           
            // --- JSON Payload ---
            List<object> jsonPayloads = new List<object>();

            if (vitalType == "blood_glucose")
            {
                jsonPayloads.Add(new
                {
                    reading_id = Guid.NewGuid().ToString("N"),
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = isFasting == true ? "Fasting" : "NonFasting",
                    data_unit = "mgdl",
                    data_value = glucose ?? 0,
                    before_meal = true,
                    event_flag = "0",
                    irregular = false
                });
            }
            else if (vitalType == "blood_pressure")
            {
                string readingId = Guid.NewGuid().ToString("N");
                jsonPayloads.Add(new
                {
                    reading_id = readingId,
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "systolic",
                    data_unit = "mmHg",
                    data_value = systolic ?? 0,
                    before_meal = false,
                    event_flag = (string?)null,
                    irregular = false
                });
                jsonPayloads.Add(new
                {
                    reading_id = readingId,
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "diastolic",
                    data_unit = "mmHg",
                    data_value = diastolic ?? 0,
                    before_meal = false,
                    event_flag = (string?)null,
                    irregular = false
                });
                jsonPayloads.Add(new
                {
                    reading_id = readingId,
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "pulse",
                    data_unit = "bpm",
                    data_value = pulse ?? 0,
                    before_meal = false,
                    event_flag = (string?)null,
                    irregular = false
                });
            }
            else if (vitalType == "body_weight")
            {
                jsonPayloads.Add(new
                {
                    reading_id = Guid.NewGuid().ToString("N"),
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = "weight",
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "Weight",
                    data_unit = "lbs",
                    data_value = weight ?? 0,
                    before_meal = false,
                    event_flag = "0",
                    irregular = false
                });
            }
            else if (vitalType == "pulse_oximeter")
            {
                jsonPayloads.Add(new
                {
                    reading_id = Guid.NewGuid().ToString("N"),
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = "pulse_ox",
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "Oxygen",
                    data_unit = "%",
                    data_value = oxygen ?? 0,
                    before_meal = false,
                    event_flag = "0",
                    irregular = false
                });
                jsonPayloads.Add(new
                {
                    reading_id = Guid.NewGuid().ToString("N"),
                    device_id = device.DeviceSerialNo,
                    device_model = device.DeviceModel,
                    date_recorded = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = nextDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = "pulse_ox",
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "Pulse",
                    data_unit = "bpm",
                    data_value = pulse ?? 0,
                    before_meal = false,
                    event_flag = "0",
                    irregular = false
                });
            }

            // --- Insret JSON data into JsonStg table---
            foreach (var payload in jsonPayloads)
            {
                StagingTableInsert(payload);            
            }
            UpdateAppJobLastBatch(device.DeviceSerialNo, nextDateTime);
        }
        private  void StagingTableInsert(object payload)
        {

            try
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

               // Console.WriteLine("JsonStg Insert begin: " + jsonData);
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsJsonStg", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@json", jsonString);
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
        private  void UpdateAppJobLastBatch(string Deviceid, DateTime newenddate)
        {
            if (newenddate <= DateTime.Now)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_AppJobLastBatch", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceSerialNo", Deviceid);
                    command.Parameters.AddWithValue("@DeviceVendorName", "iGlucose");
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
    }
    class DeviceIDs
    {
        public DeviceIDs(string _deviceSerialNo,int _deviceTypeId,string _deviceModel, DateTime _deviceActivatedDateTime)
        {
            DeviceSerialNo = _deviceSerialNo;
            DeviceTypeId = _deviceTypeId;
            DeviceModel = _deviceModel;
            DeviceActivatedDateTime = _deviceActivatedDateTime;

        }
        public string DeviceSerialNo { get; set; }
        public int DeviceTypeId { get; set; }
        public string DeviceModel { get; set; }
        public DateTime DeviceActivatedDateTime { get; set; }

    }
}
