
using Microsoft.Data.SqlClient;
using System.Data;
using static RPMPatientCommonBillingJob.ModelClasses;

namespace RPMPatientBillingJob
{
    public class BillingProcessMgr
    {
        public void BillingProcess(string cs)
        {
            try
            {
                {
                    List<SystemConfigInfo> lstConfig = GetSystemConfig(cs, "Billing", "User");
                    SystemConfigInfo provider = lstConfig.Find(x => x.Name.Equals("BillingType"));
                    Console.WriteLine(@"Billing Job Processing Started  - Time" + DateTime.UtcNow);
                    RPMPatientBilling.PatientBilling.RPMCycleBasedBilling rpm = new RPMPatientBilling.PatientBilling.RPMCycleBasedBilling();
                    if (provider.Value == "30days")
                    {
                        RPMPatientBilling.PatientBilling.RPMDaysBasedBilling rpmDays = new RPMPatientBilling.PatientBilling.RPMDaysBasedBilling();
                        rpm.GenaratePateintBillingCount(cs);
                        rpmDays.UpdatePatientBilling(cs);
                        Console.WriteLine(@"BillingProcess Initializing  - Running finish time" + DateTime.UtcNow);
                    }
                    else if (provider.Value == "cycle")
                    {
                        RPMPatientBilling.PatientBilling.RPMCycleBasedBilling rpmCycle = new RPMPatientBilling.PatientBilling.RPMCycleBasedBilling();
                        rpm.GenaratePateintBillingCount(cs);
                        rpmCycle.UpdatePatientBilling(cs);
                        Console.WriteLine(@"BillingProcess Initializing  - Running finish time" + DateTime.UtcNow);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static List<SystemConfigInfo> GetSystemConfig(string cs, string category, string createdby)
        {
            List<SystemConfigInfo> ret = new List<SystemConfigInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    //string query = "select * from SystemConfigurations where Category='iGlucose'";
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
            catch
            {
                throw;
            }
            return ret;
        }
    }
}
