
namespace RPMPatientBilling.Model
{
    public class BillingData
    {
        public BillingData() { BilledDuration = 0; }
        public int PatientId { get; set; }

        public int PatientProgramId { get; set; }
        public int BillingCodeId { get; set; }

        public DateTime? StartDate { get; set; } 
        public DateTime? BilledDate { get; set; }
        public int Totalreadings { get; set; }

        public bool TargetMet { get; set; }
        public bool ReadyToBill { get; set; }
        public int BilledDuration { get; set; }
        public int BilledReading { get; set; }
    }

    public class BillingDataUI
    {
        public int PatientId { get; set; }

        public int PatientProgramId { get; set; }
        public int BillingCodeId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime BilledDate { get; set; }
        public int Totalreadings { get; set; }

        public bool TargetMet { get; set; }
        public bool ReadyToBill { get; set; }
        public bool OnHold { get; set; }

        public bool IsPast { get; set; }

        public bool IsFuture { get; set; }

        public bool IsLastMonth { get; set; }

        public bool IsToday { get; set; }

        public bool IsCurrentMonth { get; set; }

        public bool MissingInfo { get; set; }
        public int Targetreading { get; set; }

        public string BillingCode { get; set; }
    }

    public class PatientDailyBillingData
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int BillingCodeId { get; set; }
        public string Status { get; set; }
        public int TotalVitalCount { get; set; }
        public int TotalDuration { get; set; }
        public int DaysCompleted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastBilledDate { get; set; }  
        public DateTime CreatedOn { get; set; }

        public BillingData ToBillingData()
        {
            BillingData inst = new BillingData();
            inst.PatientId = this.PatientId;
            inst.PatientProgramId = this.PatientProgramId;
            inst.BillingCodeId  = this.BillingCodeId;
            inst.BilledDuration = 0;
            inst.StartDate = this.StartDate != null ? this.StartDate : null;
            inst.BilledDate = this.LastBilledDate != null ? this.LastBilledDate : null;
            if (BillingCodeId == 1 || BillingCodeId == 2)
                inst.Totalreadings = this.TotalVitalCount;
            else if (BillingCodeId == 3 || BillingCodeId == 4 ||BillingCodeId == 6 ||BillingCodeId == 7 ||BillingCodeId == 8 ||BillingCodeId == 9 ||BillingCodeId == 10 ||BillingCodeId == 11 ||BillingCodeId == 12 ||BillingCodeId == 13 ||BillingCodeId == 14 ||BillingCodeId == 15)
                inst.Totalreadings = this.TotalDuration;
            else
                inst.Totalreadings = 0;
            inst.TargetMet = false;
            inst.ReadyToBill = false;
            inst.BilledReading = 0;
            return inst;
        }

    }

    public class PatientStartDate
    {
        public DateTime? StartDate { get; set; }

        public string Status { get; set; }
    }
    public class SystemConfigInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descripiton { get; set; }

    }
    public class Dates
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Totalreading { get; set; }
    }

}
