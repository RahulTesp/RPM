using Microsoft.Data.SqlClient;
using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Data;
using System.Reflection;
using static RPMPatientBilling.PatientBilling.RPMBilling;

namespace RPMPatientBilling.PatientBilling
{
    public class BCPT99458:IBillingResult
    {

        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99458";
        public BCPT99458(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
        }
        public void Execute(object inputData)
        {
            taskdata task = inputData as taskdata;
            int patientId = 0;
            try
            {
                List<PatientDailyBillingData> patientDailyBillingData = new List<PatientDailyBillingData>();
                patientDailyBillingData = billing.GetPatientBillingCounts(billingCode.BillingCodeID, con);
                foreach (PatientDailyBillingData patientDailyBilling in patientDailyBillingData)
                {

                    PatientProgramData patientProgramData = new PatientProgramData();
                    patientProgramData = billing.GetPatientDetails(patientDailyBilling.PatientId, patientDailyBilling.PatientProgramId, con);
                    Console.WriteLine("ProcessCPT99458 Started  " + patientProgramData.PatienttId + "");
                    patientId = patientProgramData.PatienttId;
                    DateTime? Startdate = patientDailyBilling.LastBilledDate == null ? patientDailyBilling.StartDate : patientDailyBilling.LastBilledDate;

                    if (Startdate != null)
                    {

                        var startOfMonth = Convert.ToDateTime(Startdate);
                        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                        DateTime Enddate = patientDailyBilling.CreatedOn;

                        if (endOfMonth == Enddate)
                        {

                            
                            TimeSpan t1 = TimeSpan.FromSeconds(patientDailyBilling.TotalDuration);
                            int TargetReadinginSec = billingCode.TargetReadings * 60;
                            /*if ((Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings && Convert.ToInt32(t1.Minutes)<60) || Convert.ToInt32(t1.Hours) == 1)
                            {
                                //set target met and readytoBill as true entry in db
                                SetData(patientProgramData, PatientInteractiontimes.Sum(s => s.Duration), true, true, lstBilleddate);
                            }*/
                            int BilledDuration = 20;
                            if (t1.TotalSeconds >= TargetReadinginSec)
                            {
                                if (t1.TotalSeconds >= (TargetReadinginSec + (BilledDuration * 60)))
                                {
                                    BilledDuration += 20;
                                }
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, true, startOfMonth, BilledDuration);
                            }
                        }
                        else
                        {


                           
                            TimeSpan t1 = TimeSpan.FromSeconds(patientDailyBilling.TotalDuration);
                            int TargetReadinginSec = billingCode.TargetReadings * 60;
                            int BilledDuration = 20;
                            if (t1.TotalSeconds >= TargetReadinginSec && endOfMonth == Enddate)
                            {
                                if (t1.TotalSeconds >= (TargetReadinginSec + (BilledDuration * 60)))
                                {
                                    BilledDuration += 20;
                                }
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, true, startOfMonth, BilledDuration);
                            }
                            else if (t1.TotalSeconds >= TargetReadinginSec && endOfMonth != Enddate)
                            {
                                if (t1.TotalSeconds >= (TargetReadinginSec + (BilledDuration * 60)))
                                {
                                    BilledDuration += 20;
                                }
                                SetData(patientProgramData, patientDailyBilling.TotalDuration, true, false, startOfMonth, BilledDuration);
                            }
                            /*if ((Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings && endMonth == Enddate)  || (Convert.ToInt32(t1.Hours) >= 1 && endMonth == Enddate))
                            {
                                //set target met and ready to bill as true entry in db
                                SetData(patientProgramData, PatientInteractiontim.Sum(s => s.Duration), true, true, stOfMonth);
                            }
                            else if ((Convert.ToInt32(t1.Minutes) >= billingCode.TargetReadings && endMonth != Enddate)  || (Convert.ToInt32(t1.Hours) >= 1 && endMonth != Enddate))
                            {
                                //set target met  as true entry in db
                                SetData(patientProgramData, PatientInteractiontim.Sum(s => s.Duration), true, false, stOfMonth);
                            }*/
                        }
                    }
                    Console.WriteLine("ProcessCPT99458 Completed  " + patientId + "");
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
