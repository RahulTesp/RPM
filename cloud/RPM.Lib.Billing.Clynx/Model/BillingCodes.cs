
namespace RPMPatientBilling.Model
{
    public class BillingCodes
    {
        public int BillingCodeID { get; set; }
        public string BillingCode { get; set; }

        public int Frequency { get;set; }

        public string FrequencyPeriod { get; set; }

        public int BillingThreshold { get; set; }

        public string BillingPeriod { get; set; }

        public int TargetReadings { get; set; }
    }
    public class BillingInfo
    {
        public string BillingCode { get; set; }
        public int Total { get; set; }
        public int TargetMet { get; set; }
        public int ReadyToBill { get; set; }
        public int MissingInfo { get; set; }
        public int OnHold { get; set; }
    }

    public class BilledDates
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    

}
