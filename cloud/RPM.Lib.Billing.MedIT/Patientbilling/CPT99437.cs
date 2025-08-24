using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace RPMPatientBilling.PatientBilling
{
    public class CPT99437 : IBilling
    {
        private readonly BillingCodes billingCode = null;
        private readonly BillingCodes billingCode491 = null;

        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99437";
        public string BillingCode491 => "99491";
        public CPT99437(string connectionString)
        {
            con = connectionString;
            billing = new BillingProcess();
            billingCode = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode).FirstOrDefault();
            billingCode491 = billing.GetBillingCodeDetails(con).Where(s => s.BillingCode == BillingCode491).FirstOrDefault();

        }
        public void Execute(object inst)
        {
            bool flagFirstCycle = true;
            BillingCountTaskData bctd = inst as BillingCountTaskData;
            if (bctd == null) return;
            //Console.WriteLine("Task 458 Executing " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
            try
            {
                object patientProgramData = bctd.patientProgramDatas;
                if (patientProgramData == null) return;

                int daysCompleted = 0;
                PatientStartDate PatientStartDate = new PatientStartDate();
                PatientStartDate = billing.GetPatientBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
                if (PatientStartDate == null)
                {
                    return;
                }
                else if(PatientStartDate.Status.ToLower()=="invalid" )
                {
                    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, daysCompleted,false,null);
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
                    //DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate).AddDays(1);
                    //Note: This will happen in the last day of billing.. In the last day, billed date will set in 
                    //the patientbilling table and +1 day will come as next start day.. So we should   
                    // calcualte the start date with respect to the billing threshold
                    int d = ((PatientProgramData)patientProgramData).PatienttId;
                    DateTime today = DateTime.UtcNow;
                    DateTime endDateTempay = BillingProcess.GetLocalTimeFromUTC((DateTime)today, con);
                    if (stDate.Date > endDateTempay && PatientStartDate.Status.ToLower()=="billeddate")
                    {
                        stDate = stDate.AddDays(-1 * billingCode.BillingThreshold);
                        Console.WriteLine(string.Format(@"StartDate Greater than today, resetting. {0} {1}, {2}",
                                          stDate.ToString(), ((PatientProgramData)patientProgramData).PatienttId, billingCode.BillingCode));
                    }
                    else if(stDate.Date > endDateTempay && PatientStartDate.Status.ToLower()=="active")
                    {
                        return;
                    }

                    DateTime Enddate = endDateTempay.Date;
                    var EnddateTemp = Enddate.AddDays(1);
                    var DateDiff = stDate - EnddateTemp;
                    daysCompleted = Math.Abs(DateDiff.Days);

                    var NextEndDate = DateTime.MinValue;

                    //int TotalDateRange = Math.Abs(DateDiff.Days / billingCode.BillingThreshold);
                    int TotalDateRange = 1;
                    if (Math.Abs(DateDiff.Days) > billingCode.BillingThreshold)
                    {
                        TotalDateRange = (Math.Abs(DateDiff.Days) % billingCode.BillingThreshold) > 0
                                          ? Math.Abs(DateDiff.Days / billingCode.BillingThreshold) + 1
                                          : Math.Abs(DateDiff.Days / billingCode.BillingThreshold);
                    }
                    var stDateTemp = stDate;
                    //Console.WriteLine("------------------");
                    for (int n = 0; n <= TotalDateRange; n++)
                    {
                        bool bCycleCompleted = true;
                        DateTime startDateNew = stDateTemp;
                        if (PatientStartDate.Status.ToLower()== "billeddate" && flagFirstCycle)
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
                        /*if (stDateTemp.Date > DateTime.UtcNow.Date)
                        {
                            stDateTemp = DateTime.UtcNow.Date;
                        }*/
                        if (NextEndDate > DateTime.UtcNow.Date)
                        {
                            NextEndDate = stDateTemp.AddDays((DateTime.UtcNow - startDateNew).Days);
                            bCycleCompleted = false;
                        }

                        DateTime nextEndDatetemp = BillingProcess.GetLocalTimeFromUTC((DateTime)NextEndDate, con);
                        DateTime nextEndDateNew = NextEndDate.AddDays(1).Date.AddSeconds(-1);
                        nextEndDateNew = BillingProcess.GetUTCFromLocalTime((DateTime)nextEndDateNew, con);
                        //DaysCompleted = Math.Abs((int)(NextEndDate - stDateTemp).TotalDays);
                        // DaysCompleted = Math.Abs((int)(startDateNew.Date - NextEndDate.Date).Days);
                        daysCompleted = Math.Abs((int)(startDateNew - nextEndDateNew).Days);
                        int daysDiff = daysCompleted;

                        DateTime dateCompare = nextEndDateNew.AddDays(-1);
                        if (DateTime.UtcNow>dateCompare)
                        {
                            daysDiff=daysDiff+1;
                        }
                        if (daysDiff == billingCode.BillingThreshold)
                        {
                            bCycleCompleted = true;
                        }
                        else
                        {
                            bCycleCompleted = false;
                        }
                        // Counting the current day also for day of completion

                        //DaysCompleted = Math.Abs((int)(NextEndDate - stDateTemp).TotalDays);
                        //DaysCompleted = Math.Abs((int)(stDateTemp - NextEndDate.Date).Days);
                        // Counting the current day also for day of completion
                        //if (DaysCompleted < billingCode.BillingThreshold)
                        //{
                        //    DaysCompleted += 1;
                        //}


                        List<Dates> Dates = new List<Dates>();
                        List<Dates> DatesNew = new List<Dates>();
                        for (int i = 0; i<daysDiff; i++)
                        {
                            if (i==0)
                            {
                                nextEndDatetemp = stdateloc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i==daysCompleted)
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


                        DateTime startDateFinal = Dates.First().StartDate;
                        DateTime endDateFinal = Dates.Last().EndDate;
                        List<PatientInteraction> PatientInteractiontim = billing.GetPatientInteractiontime(patientProgramData, con, startDateFinal, endDateFinal).ToList();
                        
                        if (PatientInteractiontim == null)
                        {
                            SetData(patientProgramData, 0, Convert.ToDateTime(startDateFinal), billingCode.BillingCodeID,
                                    daysCompleted+1, bCycleCompleted, null);
                        }
                        
                        int TotalReading = PatientInteractiontim.Sum(s => s.Duration);
                        if (PatientInteractiontim.Count == 0 || PatientInteractiontim ==null)
                        {
                            
                                TotalReading=0;
                        }

                        int remainingREading = 0;
                        if (TotalReading > (billingCode491.TargetReadings*60))
                        {
                            remainingREading = TotalReading- (billingCode491.TargetReadings*60);
                        }
                        SetData(patientProgramData, remainingREading,
                                Convert.ToDateTime(startDateFinal), billingCode.BillingCodeID,
                                daysDiff, bCycleCompleted, null);
                        if (bCycleCompleted)
                        {
                            PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData()
                            {
                                PatientId = ((PatientProgramData)patientProgramData).PatienttId,
                                PatientProgramId = ((PatientProgramData)patientProgramData).PatientProgramid,
                                BillingCodeId = billingCode.BillingCodeID,
                                Status = ((PatientProgramData)patientProgramData).Status,
                                TotalVitalCount = 0,
                                TotalDuration = remainingREading,
                                StartDate = startDateFinal,
                                DaysCompleted = daysDiff,
                                LastBilledDate = (DateTime?)NextEndDate,
                                CreatedOn = DateTime.UtcNow
                            };
                            RPMDaysBasedBilling rPMBilling = new RPMDaysBasedBilling();
                            rPMBilling.UpdatePatientBilledData(patientDailyBillingData, billingCode491, startDateFinal, con);
                        }
                        stDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)NextEndDate.AddDays(1), con);
                        flagFirstCycle = false;
                       // stDateTemp = NextEndDate.AddDays(1);
                    }
                    //Console.WriteLine("------------------");
                    /* DateTime Enddate = DateTime.Today;
                     var stOfMonth = new DateTime(Enddate.Year, Enddate.Month, 1);

                     if (stOfMonth.Month == Convert.ToDateTime(PatientStartDate.StartDate).Month)
                     {
                         stOfMonth = Convert.ToDateTime(PatientStartDate.StartDate);
                     }
                     DateTime endMonth = stOfMonth.AddMonths(1).AddDays(-1);
                     var daysDiff = endMonth - Enddate;
                     daysCompleted = Math.Abs(daysDiff.Days);

                     List<PatientInteraction> PatientInteractiontim = billing.GetPatientInteractiontime(patientProgramData, con).Where(s => s.Date >= stOfMonth && s.Date <= Enddate).ToList();
                     if (PatientInteractiontim == null)
                     {
                         SetData(patientProgramData, 0, Convert.ToDateTime(stOfMonth), billingCode.BillingCodeID, daysCompleted);
                     }
                     var isEstablishedExist = PatientInteractiontim.Where(s => s.IsCallNote == 1).ToList();
                     if (isEstablishedExist.Count() > 0)
                     {
                         isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
                         if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
                         {
                             PatientInteractiontim.RemoveAll(s => s.IsCallNote == 1);
                         }
                     }
                     SetData(patientProgramData, PatientInteractiontim.Sum(s => s.Duration), Convert.ToDateTime(stOfMonth), billingCode.BillingCodeID, daysCompleted);
                    */
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
                    //Console.WriteLine("Task 458 Triggered event " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
                }

            }
           // Console.WriteLine("Task 458 Execution over " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);

        }

        public void SetData(object PatientProgramDatas, int totalReadings, DateTime? startDate, int billingCodeId,int daysCompleted,
                           bool CycleCompleted, DateTime? LastBilledDate)
        {
            try
            {

                PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData();
                patientDailyBillingData.PatientId = ((PatientProgramData)PatientProgramDatas).PatienttId;
                patientDailyBillingData.PatientProgramId = ((PatientProgramData)PatientProgramDatas).PatientProgramid;
                patientDailyBillingData.BillingCodeId = billingCodeId;
                patientDailyBillingData.Status = ((PatientProgramData)PatientProgramDatas).Status;
                patientDailyBillingData.TotalVitalCount = 0;
                patientDailyBillingData.TotalDuration = totalReadings;
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
