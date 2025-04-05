
namespace RPMPatientBilling.Interface
{
    public interface IBillingResult
    {
        string BillingCode { get; }
        void Execute(object inputData);
        void SetData(object PatientProgramDatas, int totalReadings,
                    bool TargetMet, bool ReadyToBill, DateTime startDate,
                    int billedDuration);
    }
}
