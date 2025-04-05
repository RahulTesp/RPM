using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Data;
using System.Reflection;
using static RPMPatientBilling.PatientBilling.RPMBilling;

namespace RPMPatientBilling.PatientBilling
{

    public class BCPT99453:IBillingResult
    {

        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99453";
        public BCPT99453(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
        }
        
        public void Execute(object inputData)
        {
            taskdata task = inputData as taskdata;
            int patuientId = 0;
            try
            {                
                List<PatientDailyBillingData> patientDailyBillingData = new List<PatientDailyBillingData>();
                patientDailyBillingData = billing.GetPatientBillingCounts(billingCode.BillingCodeID, con);
                foreach(PatientDailyBillingData patientDailyBilling in patientDailyBillingData)
                {
                    
                    PatientProgramData patientProgramData = new PatientProgramData();
                    patientProgramData = billing.GetPatientDetails(patientDailyBilling.PatientId, patientDailyBilling.PatientProgramId,con);
                    Console.WriteLine("ProcessCPT99453 Started  " + patientProgramData.PatienttId + "");
                    patuientId= patientProgramData.PatienttId;
                    DateTime? Startdate = patientDailyBilling.StartDate;
                    if (patientDailyBilling.LastBilledDate == null ||  DateTime.Today < Convert.ToDateTime( patientDailyBilling.LastBilledDate).AddYears(1))
                    {
                        if (Startdate != null)
                        {
                            DateTime Enddate = patientDailyBilling.CreatedOn;
                            //var EnddateTemp = Enddate.AddDays(1);
                            //var DateDiff = Convert.ToDateTime(Startdate) - Enddate;
                            //if (Math.Abs(DateDiff.Days) > billingCode.BillingThreshold)
                            //{
                            //    Startdate = EnddateTemp.AddDays(-billingCode.BillingThreshold);
                            //}

                            //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= Startdate && s.ReadingDate <= Enddate).ToList();
                            
                            if (patientDailyBilling.TotalVitalCount != 0 && patientDailyBilling.TotalVitalCount >= patientProgramData.TargetReading)
                            {
                                SetData(patientProgramData, patientDailyBilling.TotalVitalCount, true, true, Convert.ToDateTime(Startdate), patientProgramData.TargetReading);

                            }
                        }
                    }



                    Console.WriteLine("ProcessCPT99453 Completed  " + patuientId + "");

                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return;
            }
            finally
            {
               
                task.manualResetEvent.Set();
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
        //public DateTime? GetBillingStartDate(object patientProgramDatas, int billingCodeId, string connectionString)
        //{
        //    try
        //    {
        //        //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();

        //            using (SqlCommand command = new SqlCommand("usp_GetBillingStartDates", con))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@PatientId", ((PatientProgramDatas)patientProgramDatas).PatienttId);
        //                command.Parameters.AddWithValue("@PatientProgramId", ((PatientProgramDatas)patientProgramDatas).PatientProgramid);
        //                command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
        //                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
        //                returnParameter.Direction = ParameterDirection.ReturnValue;
        //                var result = command.ExecuteScalar();
        //                con.Close();
        //                return result as DateTime?;

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //        return null;
        //    }
       // }

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

    }

}
