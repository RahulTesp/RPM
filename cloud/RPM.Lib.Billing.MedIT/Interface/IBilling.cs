using RPMPatientBilling.Model;

namespace RPMPatientBilling.Interface
{
    public class BillingCountTaskData
    {

        public BillingCountTaskData(ManualResetEvent _manualResetEvent,
                        PatientProgramData _ppd)
        {

            this.manualResetEvent = _manualResetEvent;
            this.patientProgramDatas = _ppd;
        }
        public PatientProgramData patientProgramDatas { get; set; }
        public ManualResetEvent manualResetEvent { get; set; }
    }
    public interface IBilling
    {
         string BillingCode { get; }
         void Execute(object patientProgramData);
        void SetData(object PatientProgramDatas, int totalReadings,
                   DateTime? startDate,int billingCodeId,int daysCompleted,
                   bool CycleCompleted, DateTime? LastBilledDate);
        
    }
}
