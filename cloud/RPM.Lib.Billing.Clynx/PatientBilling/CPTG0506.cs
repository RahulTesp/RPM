using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Reflection;

namespace RPMPatientBilling.PatientBilling
{
    public class CPTG0506 : IBilling
    {

        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "G0506";
        public CPTG0506(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
        }
        public void Execute(object inst)
        {
            BillingCountTaskData bctd = inst as BillingCountTaskData;

            if (bctd == null) return;
            //Console.WriteLine("Task 453 Executing " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
            try
            {
                object patientProgramData = bctd.patientProgramDatas;
                if (patientProgramData == null) return;
                int DaysCompleted = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = billing.GetBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
                if (PatientStartDate == null  ||
                    PatientStartDate.Status.ToLower() == "billeddate"||
                    PatientStartDate.StartDate> DateTime.UtcNow)
                {
                    return;
                    // SetData(patientProgramData, 0, null, billingCode.BillingCodeID, 0, true, PatientStartDate.StartDate);
                }
                else if (PatientStartDate.Status.ToLower() == "invalid")
                {
                    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted, false, null);
                }
                else
                {
                    var Startdate = PatientStartDate.StartDate;
                    DateTime StartDateTemp = (DateTime)Startdate;
                    StartDateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)StartDateTemp, con);
                    DateTime startDateLoc = StartDateTemp;
                    DateTime Enddate = BillingProcess.GetLocalTimeFromUTC((DateTime)DateTime.UtcNow, con);
                    DateTime EnddateTemp = (DateTime)Startdate?.AddDays(billingCode.BillingThreshold-1);

                    EnddateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)EnddateTemp, con);
                    DateTime endDateLoc = EnddateTemp;

                    //var EnddateTemp = Enddate.AddDays(1);

                    var DateDiffDays = Convert.ToDateTime(StartDateTemp).Date - Enddate.Date;
                    int DaysCompletedtotal = Math.Abs(DateDiffDays.Days);
                    var DateDiff = Convert.ToDateTime(StartDateTemp).Date - EnddateTemp.Date;
                    DaysCompleted = Math.Abs(DateDiff.Days);
                    SetData(patientProgramData, 0, Convert.ToDateTime(Startdate),
                            billingCode.BillingCodeID, DaysCompletedtotal+1, false, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
            finally
            {
                if (bctd.manualResetEvent != null)
                {
                    bctd.manualResetEvent.Set();
                    // Console.WriteLine("Task 453 Triggered event " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
                }
            }
            //Console.WriteLine("Task 453 Execution over " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);

        }
        //public void Execute(object inst)
        //{
        //    BillingCountTaskData bctd = inst as BillingCountTaskData;

        //    if (bctd == null) return;
        //    //Console.WriteLine("Task 453 Executing " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
        //    try
        //    {
        //        object patientProgramData = bctd.patientProgramDatas;
        //        if (patientProgramData == null) return;
        //        int DaysCompleted = 0;
        //        PatientStartDate PatientStartDate = new PatientStartDate();
        //        PatientStartDate = billing.GetBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
        //        if (PatientStartDate == null  || 
        //            PatientStartDate.Status.ToLower() == "billeddate"||
        //            PatientStartDate.StartDate> DateTime.UtcNow.Date)
        //        {
        //            return;
        //            // SetData(patientProgramData, 0, null, billingCode.BillingCodeID, 0, true, PatientStartDate.StartDate);
        //        }
        //        else if (PatientStartDate.Status.ToLower() == "invalid")
        //        {
        //            SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted,false, null);
        //        }
        //        else
        //        {
        //            var Startdate = PatientStartDate.StartDate;
        //            DateTime Enddate = DateTime.UtcNow;
        //            //var EnddateTemp = Enddate.AddDays(1);
        //            var DateDiff = Convert.ToDateTime(Startdate).Date - Enddate.Date;
        //            DaysCompleted = Math.Abs(DateDiff.Days);
        //            // Counting the current day also for day of completion
        //            // No need to compate with threashold as its yearly data 
        //            DaysCompleted += 1;
        //            DateTime lastDate = (DateTime)Startdate?.AddDays(billingCode.BillingThreshold -1);
        //            List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= Startdate && s.ReadingDate <= lastDate).ToList();

        //            SetData(patientProgramData, VitalReadings.Count, Convert.ToDateTime(Startdate),
        //                    billingCode.BillingCodeID, DaysCompleted,false, Enddate);
        //            /* if (Math.Abs(DateDiff.Days) > billingCode.BillingThreshold)
        //             {
        //                 Startdate = EnddateTemp.AddDays(-billingCode.BillingThreshold);
        //                 DaysCompleted = billingCode.BillingThreshold;
        //             }
        //             List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= Startdate && s.ReadingDate <= Enddate).ToList();
        //             if (VitalReadings == null)
        //             {
        //                 SetData(patientProgramData, 0, Convert.ToDateTime(Startdate), billingCode.BillingCodeID, DaysCompleted);
        //             }
        //             else
        //             {
        //                 SetData(patientProgramData, VitalReadings.Count, Convert.ToDateTime(Startdate), billingCode.BillingCodeID, DaysCompleted);
        //             }*/
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //    }
        //    finally
        //    {
        //        if(bctd.manualResetEvent != null)
        //        {
        //            bctd.manualResetEvent.Set();
        //           // Console.WriteLine("Task 453 Triggered event " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
        //        }
        //    }
        //    //Console.WriteLine("Task 453 Execution over " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);

        //}

        public void SetData(object PatientProgramDatas, int totalReadings,
                            DateTime? startDate, int billingCodeId, int dayscompleted,
                            bool CycleCompleted, DateTime? LastBilledDate)
        {
            try
            {

                PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData();
                patientDailyBillingData.PatientId= ((PatientProgramData)PatientProgramDatas).PatienttId;
                patientDailyBillingData.PatientProgramId = ((PatientProgramData)PatientProgramDatas).PatientProgramid;
                patientDailyBillingData.BillingCodeId= billingCodeId;
                patientDailyBillingData.Status= ((PatientProgramData)PatientProgramDatas).Status;
                patientDailyBillingData.TotalVitalCount = totalReadings;
                patientDailyBillingData.TotalDuration = 0;
                patientDailyBillingData.StartDate = startDate;
                patientDailyBillingData.DaysCompleted = dayscompleted;
                patientDailyBillingData.LastBilledDate = CycleCompleted ? LastBilledDate : null;
                patientDailyBillingData.CreatedOn = DateTime.UtcNow;
                billing.SavePatientBillingData(patientDailyBillingData, con);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
        }

        //public void SetData(object PatientProgramDatas, int totalReadings,
        //                    DateTime? startDate,int billingCodeId,int dayscompleted,
        //                    bool CycleCompleted, DateTime? LastBilledDate)
        //{
        //    try
        //    {

        //        PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData();
        //        patientDailyBillingData.PatientId= ((PatientProgramData)PatientProgramDatas).PatienttId;
        //        patientDailyBillingData.PatientProgramId = ((PatientProgramData)PatientProgramDatas).PatientProgramid;
        //        patientDailyBillingData.BillingCodeId= billingCodeId;
        //        patientDailyBillingData.Status= ((PatientProgramData)PatientProgramDatas).Status;
        //        patientDailyBillingData.TotalVitalCount = totalReadings;
        //        patientDailyBillingData.TotalDuration = 0;
        //        patientDailyBillingData.StartDate = startDate;
        //        patientDailyBillingData.DaysCompleted = dayscompleted;
        //        patientDailyBillingData.LastBilledDate = CycleCompleted ? LastBilledDate : null;
        //        patientDailyBillingData.CreatedOn = DateTime.UtcNow.Date;
        //        billing.SavePatientBillingData(patientDailyBillingData, con);
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //    }
        //}

    }
}
