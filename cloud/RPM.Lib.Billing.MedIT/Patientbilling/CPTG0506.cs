using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPMPatientBilling.PatientBilling
{
    public class CPTG0506 : IBilling
    {

        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con=string.Empty;
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
                if(patientProgramData == null) return;
                int DaysCompleted = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = billing.GetBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
                if (PatientStartDate == null  || 
                    PatientStartDate.Status.ToLower() == "billeddate"||
                    PatientStartDate.StartDate> DateTime.UtcNow.Date)
                {
                    return;
                    // SetData(patientProgramData, 0, null, billingCode.BillingCodeID, 0, true, PatientStartDate.StartDate);
                }
                else if (PatientStartDate.Status.ToLower() == "invalid")
                {
                    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted,false, null);
                }
                else
                {
                    DateTime Startdate = Convert.ToDateTime( PatientStartDate.StartDate).AddDays(1);
                    DateTime StartDateTemp = (DateTime)Startdate;
                    StartDateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)StartDateTemp, con);
                    DateTime startDateLoc = StartDateTemp;


                    DateTime Enddate = DateTime.UtcNow;

                    DateTime EnddateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)Enddate.AddDays(1), con);
                    
                    

                    var DateDiffDays = Convert.ToDateTime(StartDateTemp).Date - Enddate.Date;
                    int DaysCompletedtotal = Math.Abs(DateDiffDays.Days);
                    var DateDiff = Convert.ToDateTime(Startdate) - Enddate;
                    DaysCompleted = Math.Abs(DateDiff.Days);


                    

                    SetData(patientProgramData, 0, Convert.ToDateTime(Startdate),
                            billingCode.BillingCodeID, DaysCompleted+1,false, Enddate);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
            finally
            {
                if(bctd.manualResetEvent != null)
                {
                    bctd.manualResetEvent.Set();
                   // Console.WriteLine("Task 453 Triggered event " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
                }
            }
            //Console.WriteLine("Task 453 Execution over " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);

        }



        public void SetData(object PatientProgramDatas, int totalReadings,
                            DateTime? startDate,int billingCodeId,int dayscompleted,
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
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
        }

    }
}
