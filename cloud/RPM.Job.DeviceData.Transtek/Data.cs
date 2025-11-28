using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azuretranstekwebjob
{
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

    public class StagingInput
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
    public class TranstekDeviceTelemetry
    {
        public string deviceId { get; set; }
        public long createdAt { get; set; }
        public object data { get; set; }
        public object deviceData { get; set; }
        public bool isTest { get; set; }
        public string modelNumber { get; set; }
        public string messageType { get; set; }
    }
    public class SystemConfigInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descripiton { get; set; }

    }
}
