using Microsoft.Data.SqlClient;
using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Data;
using System.Reflection;
using static RPMPatientBilling.PatientBilling.RPMBilling;

namespace RPMPatientBilling.PatientBilling
{
    public class BCPT99457:IBillingResult
    {

        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99457";
        public BCPT99457(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
        }
        public void Execute(object inputData)
        {
            int patientId = 0;
            taskdata task = inputData as taskdata;

            try
            {
                List<PatientDailyBillingData> patientDailyBillingData = new List<PatientDailyBillingData>();
                patientDailyBillingData = billing.GetPatientBillingCounts(billingCode.BillingCodeID, con);
                foreach (PatientDailyBillingData patientDailyBilling in patientDailyBillingData)
                {

                    PatientProgramData patientProgramData = new PatientProgramData();
                    patientProgramData = billing.GetPatientDetails(patientDailyBilling.PatientId, patientDailyBilling.PatientProgramId, con);
                    Console.WriteLine("ProcessCPT99457 Started  " + patientProgramData.PatienttId + "");
                    patientId = patientProgramData.PatienttId;
                    DateTime? Startdate = patientDailyBilling.LastBilledDate == null ? patientDailyBilling.StartDate : Convert.ToDateTime(patientDailyBilling.LastBilledDate).AddDays(1);
                    if (Startdate != null)
                    {

                        var startOfMonth = Convert.ToDateTime(Startdate);
                        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                        DateTime Enddate = patientDailyBilling.CreatedOn;

                        if (endOfMonth == Enddate)
                        {


                            TimeSpan t1 = TimeSpan.FromSeconds(patientDailyBilling.TotalDuration);
                            if (Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings || Convert.ToInt32(t1.Minutes) >= 1)
                            {
                                //set target met and readytoBill as true entry in db
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, true, startOfMonth, billingCode.TargetReadings);

                            }
                        }
                        else
                        {
                           

                            TimeSpan t1 = TimeSpan.FromSeconds(patientDailyBilling.TotalDuration);
                            if ((Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings && endOfMonth == Enddate) || (Convert.ToInt32(t1.Hours) >= 1 && endOfMonth == Enddate))
                            {
                                //set target met and ready to bill as true entry in db
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, true, startOfMonth, billingCode.TargetReadings);

                            }
                            else if ((Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings && endOfMonth != Enddate) || (Convert.ToInt32(t1.Hours) >= 1 && endOfMonth != Enddate))
                            {
                                //set target met  as true entry in db
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, false, startOfMonth, billingCode.TargetReadings);
                            }
                        }
                    }

                    Console.WriteLine("ProcessCPT99457 Completed  " + patientId + "");


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
            finally
            {
                
                task.manualResetEvent.Set();
            }
        }

        public DateTime? GetPatientBillingStartDate(object patientProgramData, int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingStartDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", ((PatientProgramDatas)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@PatientProgramId", ((PatientProgramDatas)patientProgramData).PatientProgramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        DateTime result = (DateTime)command.ExecuteScalar();
                        con.Close();
                        return result;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }

        public string GetPatientLatestStatus(object patientProgramData, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientLatestSatatus", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientId", ((PatientProgramDatas)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramDatas)patientProgramData).PatientProgramid);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        string status = (string)command.ExecuteScalar();
                        con.Close();
                        return status;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public void SetData(object PatientProgramDatas, int totalReadings, bool targetMet, bool readyToBill, DateTime startDate, int billedDuration)
        {
            try
            {
                BillingData billingData = new BillingData();
                billingData.PatientId = ((PatientProgramData)PatientProgramDatas).PatienttId;
                billingData.PatientProgramId = ((PatientProgramData)PatientProgramDatas).PatientProgramid;
                billingData.BillingCodeId = billingCode.BillingCodeID;
                billingData.StartDate = startDate;
                billingData.BilledDate = DateTime.Now;
                billingData.Totalreadings = totalReadings;
                billingData.TargetMet = targetMet;
                billingData.ReadyToBill = readyToBill;
                billingData.BilledDuration = billedDuration;
                billing.SavePatientBillingResult(billingData, con);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
        }

    }
}
