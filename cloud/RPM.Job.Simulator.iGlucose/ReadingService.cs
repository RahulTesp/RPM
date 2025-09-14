using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace RPM.Job.Simulator.iGlucose
{

    internal class ReadingService
    {
        private readonly string _connectionString;

        public ReadingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        //Fetch the devices form the Device table with DeviceStatus = "Active" 
        public void getandProcessActiveDevices()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                //Console.WriteLine("Fetching active devices");

                var devices = new List<(string DeviceSerialNo, int DeviceTypeId, string DeviceModel)>();

                string deviceQuery = @"
                    SELECT d.DeviceSerialNo, d.DeviceTypeId, dm.Name AS DeviceModel
                    FROM dbo.Device d
                    INNER JOIN dbo.DeviceModel dm ON d.DeviceModelId = dm.Id
                    WHERE d.DeviceStatus = 'Active';";

                using SqlCommand getDevicesCmd = new SqlCommand(deviceQuery, conn);

                using (SqlDataReader reader = getDevicesCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string deviceSerialNo = reader["DeviceSerialNo"].ToString()!;
                        int deviceTypeId = Convert.ToInt32(reader["DeviceTypeId"]);
                        string deviceModel = reader["DeviceModel"].ToString()!;
                        devices.Add((deviceSerialNo, deviceTypeId, deviceModel));
                    }
                }

                foreach (var device in devices)
                {
                    Console.WriteLine($"Processing Device: {device.DeviceSerialNo}, TypeId: {device.DeviceTypeId}, Model: {device.DeviceModel}");
                    GenerateAndInsertReading(conn, device.DeviceSerialNo, device.DeviceModel, device.DeviceTypeId);
                }
            }
        }

        private void GenerateAndInsertReading(SqlConnection conn, string deviceSerialNo, string deviceModel, int deviceTypeId)
        {
            Random rnd = new Random();
            DateTime now = DateTime.Now;

            string vitalType;
            int? systolic = null, diastolic = null, pulse = null, glucose = null;
            decimal? weight = null;
            int? oxygen = null;

            bool? isFasting = null;

            //Time zone offset from system
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            string timeZoneOffset = $"{(offset.Hours >= 0 ? "+" : "-")}{offset.Hours:00}:{offset.Minutes:00}";

            switch (deviceTypeId)
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
                    Console.WriteLine($"Skipping unknown device type: {deviceTypeId}");
                    return;
            }

            // --- Insert into DeviceReadings table ---
            using (SqlCommand cmd = new SqlCommand("dbo.InsertDeviceReading", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReadingDateTime", now);
                cmd.Parameters.AddWithValue("@VitalType", vitalType);
                cmd.Parameters.AddWithValue("@Systolic", (object?)systolic ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Diastolic", (object?)diastolic ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pulse", (object?)pulse ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Fasting", vitalType == "blood_glucose" && isFasting == true ? glucose : DBNull.Value);
                cmd.Parameters.AddWithValue("@NonFasting", vitalType == "blood_glucose" && isFasting == false ? glucose : DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", (object?)weight ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Oxygen", (object?)oxygen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Temperature", DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            // --- JSON Payload ---
            List<object> jsonPayloads = new List<object>();

            if (vitalType == "blood_glucose")
            {
                jsonPayloads.Add(new
                {
                    reading_id = Guid.NewGuid().ToString("N"),
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
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
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "systolic",
                    data_unit = (string?)null,
                    data_value = systolic ?? 0,
                    before_meal = false,
                    event_flag = (string?)null,
                    irregular = false
                });
                jsonPayloads.Add(new
                {
                    reading_id = readingId,
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "diastolic",
                    data_unit = (string?)null,
                    data_value = diastolic ?? 0,
                    before_meal = false,
                    event_flag = (string?)null,
                    irregular = false
                });
                jsonPayloads.Add(new
                {
                    reading_id = readingId,
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "pulse",
                    data_unit = (string?)null,
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
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "Weight",
                    data_unit = "kg",
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
                    device_id = deviceSerialNo,
                    device_model = deviceModel,
                    date_recorded = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    date_received = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reading_type = vitalType,
                    battery = rnd.Next(50, 100),
                    time_zone_offset = timeZoneOffset,
                    data_type = "Oxygen",
                    data_unit = "%",
                    data_value = oxygen ?? 0,
                    before_meal = false,
                    event_flag = "0",
                    irregular = false
                });
            }

            // --- Insret JSON data into JsonStg table---
            foreach (var payload in jsonPayloads)
            {
                string jsonString = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                using (SqlCommand cmd = new SqlCommand("dbo.InsertJsonStg", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Json", jsonString);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
