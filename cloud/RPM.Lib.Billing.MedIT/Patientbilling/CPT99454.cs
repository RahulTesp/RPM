using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RPMPatientBilling.PatientBilling
{
    public class CPT99454 : IBilling
    {
        private readonly BillingCodes billingCode = null;
        private BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99454";
        public CPT99454(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
        }
        public void Execute(object inst)
        {
            bool flagFirstCycle = true;
            BillingCountTaskData bctd = inst as BillingCountTaskData;
            if (bctd == null) return;
            //Console.WriteLine("Task 454 Executing " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
            try
            {
                object patientProgramData = bctd.patientProgramDatas;
                if (patientProgramData == null) return;

                int DaysCompleted = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = billing.GetPatientBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
                if (PatientStartDate == null)
                {
                    return;
                }
                else if (PatientStartDate.Status.ToLower()=="invalid")
                {
                    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted,false,null);
                }
                else
                {
                    DateTime stDate = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                         stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    }
                    else
                    {
                        stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    }
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold


                    DateTime today = DateTime.UtcNow;
                    DateTime todayTemp= BillingProcess.GetLocalTimeFromUTC((DateTime)today, con);
                    if (stDate.Date > todayTemp && PatientStartDate.Status.ToLower() == "billeddate")
                    {
                        stDate = stDate.AddDays(-1 * billingCode.BillingThreshold);
                        Console.WriteLine(string.Format(@"StartDate Greater than today, resetting. {0} {1}, {2}",
                                          stDate.ToString(), ((PatientProgramData)patientProgramData).PatienttId, billingCode.BillingCode));
                    }
                    else if (stDate.Date > todayTemp && PatientStartDate.Status.ToLower() == "active")
                    {
                        return;
                    }


                        DateTime Enddate = todayTemp.Date;
                    var EnddateTemp = Enddate.AddDays(1);
                    var DateDiff = stDate - EnddateTemp;
                    DaysCompleted = Math.Abs(DateDiff.Days);

                    var NextEndDate = DateTime.MinValue;

                    //int TotalDateRange = Math.Abs(DateDiff.Days / billingCode.BillingThreshold);
                    int TotalDateRange = 1;
                    if (Math.Abs(DateDiff.Days)>billingCode.BillingThreshold)
                    {
                        TotalDateRange = (Math.Abs(DateDiff.Days) % billingCode.BillingThreshold) > 0
                                          ? Math.Abs(DateDiff.Days / billingCode.BillingThreshold) + 1
                                          : Math.Abs(DateDiff.Days / billingCode.BillingThreshold);
                    }
                    var stDateTemp = stDate;
                    
                    for (int n = 0; n <= TotalDateRange; n++)
                    {
                        bool bCycleComplete = true;
                        //if(!flagFirstCycle)
                        //    stDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)stDateTemp, con);

                        

                        DateTime startDateNew = stDateTemp;
                        if(PatientStartDate.Status.ToLower()== "billeddate" && flagFirstCycle)
                        {
                            stDateTemp=BillingProcess.GetUTCFromLocalTime((DateTime)stDateTemp, con);
                            startDateNew = stDateTemp;
                        }
                        if (stDateTemp >DateTime.UtcNow) 
                        {
                            break;
                        }
                        stDateTemp=BillingProcess.GetLocalTimeFromUTC((DateTime)stDateTemp, con);
                        DateTime stdateloc = stDateTemp;
                        NextEndDate = stDateTemp.Date.AddDays(billingCode.BillingThreshold-1);

                        if (NextEndDate>DateTime.UtcNow.Date)
                        {
                            NextEndDate = stDateTemp.AddDays((DateTime.UtcNow - startDateNew).Days);
                            bCycleComplete = false;
                        }

                        DateTime nextEndDatetemp = DateTime.MinValue;
                       
                        nextEndDatetemp = BillingProcess.GetLocalTimeFromUTC((DateTime)NextEndDate, con);
                        DateTime nextEndDateNew = NextEndDate.AddDays(1).Date.AddSeconds(-1);
                        nextEndDateNew = BillingProcess.GetUTCFromLocalTime((DateTime)nextEndDateNew, con);
                        //DaysCompleted = Math.Abs((int)(NextEndDate - stDateTemp).TotalDays);

                        DaysCompleted = Math.Abs((int)(startDateNew - nextEndDateNew).Days);
                        int daysDiff = DaysCompleted;
                        
                        DateTime dateCompare = nextEndDateNew.AddDays(-1);
                        if(DateTime.UtcNow>dateCompare)
                        {
                            daysDiff=daysDiff+1;
                        }
                        if (daysDiff == billingCode.BillingThreshold)
                        {
                            bCycleComplete = true;
                        }
                        else
                        {
                            bCycleComplete = false;
                        }
                        // Counting the current day also for day of completion
                        //if (daysDiff< billingCode.BillingThreshold )
                        //{
                        //    daysDiff += 1;
                        //}


                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();

                        for (int i = 0; i<daysDiff; i++)
                        {
                            if (i==0)
                            {
                                nextEndDatetemp = stdateloc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i==DaysCompleted)
                            {
                                stDateTemp = stdateloc.AddDays(i).Date;
                                nextEndDatetemp = stDateTemp.AddDays(1).Date.AddSeconds(-1);
                            }
                            else
                            {
                                stDateTemp = stdateloc.AddDays(i).Date;
                                nextEndDatetemp = stDateTemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            stDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)stDateTemp, con);
                            nextEndDatetemp = BillingProcess.GetUTCFromLocalTime((DateTime)nextEndDatetemp, con);


                            Dates.Add(new Dates() { StartDate = (DateTime)stDateTemp, EndDate = nextEndDatetemp, Totalreading=0 });




                        }


                        if (Dates.Count>0)
                        {

                            foreach (Dates date in Dates)
                            {

                                List<VitalReading> VitalReadings = billing.GetVitalReadingsLocal(patientProgramData, con, date.StartDate, date.EndDate).ToList();

                                if (VitalReadings.Count>0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=0 });
                                }

                            }


                        }


                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();






























                        if(DateTime.UtcNow> Dates.Last().StartDate )
                        {

                        }

                        //int days = DaysCompleted = Math.Abs((int)(startDateNew - NextEndDate.Date.AddDays(1)).Days);
                        // List<VitalReading> VitalReadings1 = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= stDateTemp && s.ReadingDate <= NextEndDate).ToList();
                        int nVitalCount = 0;
                       
                        nVitalCount = VitalCount;
                        
                        SetData(patientProgramData, nVitalCount, startDateNew, billingCode.BillingCodeID,
                                daysDiff, bCycleComplete, null);
                        if (bCycleComplete)
                        {
                            PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData()
                            {
                                PatientId = ((PatientProgramData)patientProgramData).PatienttId,
                                PatientProgramId = ((PatientProgramData)patientProgramData).PatientProgramid,
                                BillingCodeId = billingCode.BillingCodeID,
                                Status = ((PatientProgramData)patientProgramData).Status,
                                TotalVitalCount = nVitalCount,
                                TotalDuration = 0,
                                StartDate = startDateNew,
                                DaysCompleted = daysDiff,
                                LastBilledDate = (DateTime?)NextEndDate,
                                CreatedOn = DateTime.UtcNow
                             };
                            RPMDaysBasedBilling rPMBilling = new RPMDaysBasedBilling();
                            rPMBilling.UpdatePatientBilledData(patientDailyBillingData, billingCode, startDateNew, con);
                        }

                        stDateTemp=BillingProcess.GetUTCFromLocalTime((DateTime)NextEndDate.AddDays(1), con); 
                        //stDateTemp=NextEndDate.AddDays(1);
                        flagFirstCycle=false;
                    }

                    //stDate = EnddateTemp.AddDays(-billingCode.BillingThreshold);
                    /* List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= Enddate).ToList();
                     if (VitalReadings == null)
                     {
                         SetData(patientProgramData, 0, stDate, billingCode.BillingCodeID, DaysCompleted);
                     }
                     else
                     {
                         SetData(patientProgramData, VitalReadings.Count, stDate, billingCode.BillingCodeID, DaysCompleted);
                     }*/

                    /* if (Math.Abs(DateDiff.Days) >= billingCode.BillingThreshold)
                     {
                         DaysCompleted = billingCode.BillingThreshold;
                         stDate = EnddateTemp.AddDays(-billingCode.BillingThreshold);
                         List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= Enddate).ToList();
                         if (VitalReadings == null)
                         {
                             SetData(patientProgramData, 0, stDate, billingCode.BillingCodeID, DaysCompleted);
                         }
                         else
                         {
                             SetData(patientProgramData, VitalReadings.Count, stDate, billingCode.BillingCodeID, DaysCompleted);
                         }
                         //set traget met and ready to bill as true
                     }
                     else
                     {
                         List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= stDate && s.ReadingDate <= Enddate).ToList();
                         if (VitalReadings == null)
                         {
                             SetData(patientProgramData, 0, stDate, billingCode.BillingCodeID, DaysCompleted);
                         }
                         else
                         {
                             SetData(patientProgramData, VitalReadings.Count, stDate, billingCode.BillingCodeID, DaysCompleted);
                         }
                         //Set target met  as true
                     }*/
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
                //    Console.WriteLine("Task 454 Triggered event " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);

                }
            }
           // Console.WriteLine("Task 454 Execution over " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);


        }



        public void SetData(object PatientProgramDatas, int totalReadings, DateTime? startDate,
                            int billingCodeId,int daysCompleted,bool CycleCompleted,
                            DateTime? LastBilledDate)
        {
            try
            {

                PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData();
                patientDailyBillingData.PatientId = ((PatientProgramData)PatientProgramDatas).PatienttId;
                patientDailyBillingData.PatientProgramId = ((PatientProgramData)PatientProgramDatas).PatientProgramid;
                patientDailyBillingData.BillingCodeId = billingCodeId;
                patientDailyBillingData.Status = ((PatientProgramData)PatientProgramDatas).Status;
                patientDailyBillingData.TotalVitalCount = totalReadings;
                patientDailyBillingData.TotalDuration = 0;
                patientDailyBillingData.StartDate = startDate;
                patientDailyBillingData.DaysCompleted = daysCompleted;
                patientDailyBillingData.LastBilledDate = CycleCompleted ? LastBilledDate : null;
                patientDailyBillingData.CreatedOn = DateTime.UtcNow;
                billing.SavePatientBillingData(patientDailyBillingData, con);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
        }
    }
}
