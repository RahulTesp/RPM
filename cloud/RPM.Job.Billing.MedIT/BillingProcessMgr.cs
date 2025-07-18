namespace RPMPatientBillingJob
{
    public class BillingProcessMgr
    {
        public void BillingProcess(string cs)
        {
            try
            {
                {
                    Console.WriteLine(@"Billing Job Processing Started  - Time" + DateTime.UtcNow);
                    RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
                    rpm.GenaratePateintBillingCount(cs);
                    rpm.UpdatePatientBilling(cs);
                    Console.WriteLine(@"Billing Job Processing Ended  - Time" + DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
        }
    }
}
