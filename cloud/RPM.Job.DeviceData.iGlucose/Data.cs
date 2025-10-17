using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;
using Newtonsoft.Json;

namespace iglucosedata
{

    class iGlucoseBP
    {
        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public DateTime date_recorded { get; set; }
        public DateTime date_received { get; set; }
        public string reading_type { get; set; }
        public int systolic_mmhg { get; set; }
        public int diastolic_mmhg { get; set; }
        public int pulse_bpm { get; set; }
        public bool before_meal { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public bool irregular { get; set; }
        public string event_flag { get; set; }
    }
    class iGlucoseBG
    {
        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public DateTime date_recorded { get; set; }
        public DateTime date_received { get; set; }
        public double blood_glucose_mgdl { get; set; }
        public double blood_glucose_mmol { get; set; }
        public bool before_meal { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public string reading_type { get; set; }
        public bool irregular { get; set; }
        public string event_flag { get; set; }

    }
    class iGlucoseW
    {
        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public DateTime date_recorded { get; set; }
        public DateTime date_received { get; set; }
        public string reading_type { get; set; }
        public double tare_kg { get; set; }
        public double weight_kg { get; set; }
        public double tare_lbs { get; set; }
        public double weight_lbs { get; set; }
        public bool before_meal { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public bool irregular { get; set; }
        public string event_flag { get; set; }
    }
    class iGlucosePulse
    {
        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public DateTime date_recorded { get; set; }
        public DateTime date_received { get; set; }
        public string reading_type { get; set; }
        public int spo2 { get; set; }
        public int pulse_bpm { get; set; }
        public bool before_meal { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public bool irregular { get; set; }
        public string event_flag { get; set; }

    }
    class DatabaseInput
    {

        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public string date_recorded { get; set; }
        public string date_received { get; set; }
        public string reading_type { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public string data_type { get; set; }
        public string data_unit { get; set; }
        public double data_value { get; set; }
        public bool before_meal { get; set; }
        public string event_flag { get; set; }
        public bool irregular { get; set; }
    }
    public class SystemConfigInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descripiton { get; set; }

    }
    class Data
    {
        public static List<SystemConfigInfo> GetSystemConfig(string cs, string category, string createdby)
        {
            List<SystemConfigInfo> ret = new List<SystemConfigInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
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
            catch (Exception EX)
            {
                throw;
            }
            return ret;
        }

        public static string CallRestMethod(string url, DateTime startdate, string deviceid, string API_Key)
        {
            try
            {
                DateTime newenddate = startdate.AddDays(2);
                if (newenddate >= DateTime.UtcNow)
                {
                    newenddate = DateTime.UtcNow;
                }
                string strstartdate = startdate.ToString("yyyy-MM-dd'T'HH:mm:ss");
                strstartdate = strstartdate.Replace('.', ':');
                string strnewenddate = newenddate.ToString("yyyy-MM-dd'T'HH:mm:ss");
                strnewenddate = strnewenddate.Replace('.', ':');
                string resp = string.Empty;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                var json = "";
                string[] arr_deviceid = new string[] { deviceid };
                HttpWebResponse httpResponse = null;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    ReqBody info = new ReqBody();
                    info.api_key = API_Key;
                    info.ingest_date_start = strstartdate;
                    info.ingest_date_end = strnewenddate;
                    info.device_ids = arr_deviceid;
                    json = JsonConvert.SerializeObject(info);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                using (httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = httpResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            resp = reader.ReadToEnd();
                        }
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex);
                return null;
            }
        }
    }
    class DeviceIDs
    {
        public DeviceIDs(string _id, DateTime _date)
        {
            Deviceid = _id;
            ActivatedDate = _date;

        }
        public string Deviceid { get; set; }
        public DateTime ActivatedDate { get; set; }

    }
    class VitalUnits
    {
        public VitalUnits(string _measure, string _unit)
        {
            MeasureName = _measure;
            UnitName = _unit;
        }
        public string MeasureName { get; set; }
        public string UnitName { get; set; }

    }
    class ReqBody
    {
        public string api_key { get; set; }
        public string ingest_date_start { get; set; }
        public string ingest_date_end { get; set; }
        public string[] device_ids { get; set; }
    }
    class ReadingQueue
    {
        public JArray jaray { get; set; }
        public string unit { get; set; }
    }
}
