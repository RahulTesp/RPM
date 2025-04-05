using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Data;
using System.Reflection;
using static RPMPatientBilling.PatientBilling.RPMBilling;

namespace RPMPatientBilling.PatientBilling
{
    public class BCPT99454 : IBillingResult
    {

        private readonly BillingCodes billingCode = null;
        private BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99454";
        public BCPT99454(string connectionString)
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
                    Console.WriteLine("ProcessCPT99454 Started  " + patientProgramData.PatienttId + "");
                    patientId = patientProgramData.PatienttId;
                    DateTime? Startdate = patientDailyBilling.LastBilledDate == null ? patientDailyBilling.StartDate : patientDailyBilling.LastBilledDate;
                    if (Startdate != null)
                    {
                        DateTime stDate = Convert.ToDateTime(Startdate);
                        DateTime Enddate = patientDailyBilling.CreatedOn;
                        var EnddateTemp = Enddate.AddDays(1);
                        var DateDiff = stDate - EnddateTemp;



                        if (Math.Abs(DateDiff.Days) >= billingCode.BillingThreshold)
                        {


                           
                            if (patientDailyBilling.TotalVitalCount != 0 && patientDailyBilling.TotalVitalCount >= patientProgramData.TargetReading)
                            {
                                //set traget met and ready to bill as true
                                SetData(patientProgramData, patientDailyBilling.TotalVitalCount, true, true, stDate, patientProgramData.TargetReading);

                            }
                        }
                        else
                        {

                            
                            if (patientDailyBilling.TotalVitalCount != 0 && patientDailyBilling.TotalVitalCount >= patientProgramData.TargetReading)
                            {
                                //Set target met  as true
                                SetData(patientProgramData, patientDailyBilling.TotalVitalCount, true, false, stDate, patientProgramData.TargetReading);

                            }
                        }
                    }
                    Console.WriteLine("ProcessCPT99454 Completed  " + patientId + "");

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

        //public DateTime? GetPatientBillingStartDate(object patientProgramData, int billingCodeId, string connectionString)
        //{
        //    try
        //    {
        //        //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();

        //            using (SqlCommand command = new SqlCommand("usp_GetPatientBillingStartDate", con))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@PatientId", ((PatientProgramDatas)patientProgramData).PatienttId);
        //                command.Parameters.AddWithValue("@PatientProgramId", ((PatientProgramDatas)patientProgramData).PatientProgramid);
        //                command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
        //                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
        //                returnParameter.Direction = ParameterDirection.ReturnValue;
        //                DateTime result = (DateTime)command.ExecuteScalar();
        //                con.Close();
        //                return result;

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //        return null;
        //    }
        //}

        //public string GetPatientLatestStatus(object patientProgramData, string connectionString)
        //{
        //    try
        //    {
        //        //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();

        //            using (SqlCommand command = new SqlCommand("usp_GetPatientLatestSatatus", con))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@patientId", ((PatientProgramDatas)patientProgramData).PatienttId);
        //                command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramDatas)patientProgramData).PatientProgramid);
        //                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
        //                returnParameter.Direction = ParameterDirection.ReturnValue;
        //                string status = (string)command.ExecuteScalar();
        //                con.Close();
        //                return status;

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //        return null;
        //    }
        //}

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
