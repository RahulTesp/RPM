
namespace RPMPatientBilling.Model
{
    public class PatientProgramData
    {
        public int PatienttId { get; set; }
        public int PatientProgramid { get; set; }
        public DateTime? ActiveDate { get; set; }

        public int TargetReading { get; set; }

        public string Status { get; set; }

    }
    public class PatientProgramDatas
    {
        public int PatienttId { get; set; }
        public int PatientProgramid { get; set; }
        public DateTime? ActiveDate { get; set; }

        public int TargetReading { get; set; }
        public string status { get; set; }

    }
    public class BillingCodesData
    {
        public int BillingCodeID { get; set; }
        public string BillingCode { get; set; }

        public int Frequency { get; set; }

        public string FrequencyPeriod { get; set; }

        public int BillingThreshold { get; set; }

        public string BillingPeriod { get; set; }

        public int TargetReadings { get; set; }
    }
    public class VitalReadings
    {
        public int programId { get; set; }
        public DateTime ReadingDate { get; set; }
        public int Totalreadings { get; set; }
    }
}
