using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Reflection;

namespace RPMPatientBilling.PatientBilling
{
    public class CPT99426 : IBilling
    {
        private readonly BillingCodes billingCode = null;
        private readonly BillingProcess billing = null;
        private readonly string con = string.Empty;
        public string BillingCode => "99426";
        public CPT99426(string connectionString)
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
            //Console.WriteLine("Task 457 Executing " + ((PatientProgramData)bctd.patientProgramDatas).PatienttId);
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
                else if (PatientStartDate.Status.ToLower() == "active")
                {
                    DateTime stDate = Convert.ToDateTime(PatientStartDate.StartDate);
                    if (stDate > DateTime.UtcNow)
                    {
                        return;
                    }
                    int monthsApart = 12 * (stDate.Year - DateTime.UtcNow.Year) + stDate.Month - DateTime.UtcNow.Month;
                    int monthDiff = Math.Abs(monthsApart);
                    DateTime startDate = stDate;
                    DateTime billedDate = DateTime.MinValue;
                    for (int n = 0; n < monthDiff; n++)
                    {
                        billedDate = RPMCycleBasedBilling.GetNextBilledDate(startDate);
                        DateTime billedDateTemp = billedDate;
                        DateTime startDateTemp = startDate;
                        DateTime todayDate = DateTime.UtcNow;
                        todayDate = BillingProcess.GetLocalTimeFromUTC((DateTime)todayDate, con);
                        if (billedDate > todayDate) continue;
                        billedDateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)billedDate, con);
                        startDateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)startDate, con);
                        int nTotDays = Math.Abs((billedDateTemp - startDateTemp).Days);
                        AddBilledData((PatientProgramData)patientProgramData, startDate, billedDate, startDateTemp, billedDateTemp, true, nTotDays + 1);
                        startDate = BillingProcess.GetUTCFromLocalTime((DateTime)billedDate.AddDays(1), con);
                    }
                    PatientStartDate = billing.GetPatientBillingStartDate(patientProgramData, billingCode.BillingCodeID, con);
                    PatientStartDate.StartDate = startDate;
                    flagFirstCycle = false;
                }
                else if (PatientStartDate.Status.ToLower()=="invalid")
                {
                    SetData(patientProgramData, 0, null, billingCode.BillingCodeID, DaysCompleted, false, null);
                }

                DateTime newStartDate = Convert.ToDateTime(PatientStartDate.StartDate);
                DateTime today = DateTime.UtcNow;
                DateTime endDateTempay = BillingProcess.GetLocalTimeFromUTC((DateTime)today, con);
                if (newStartDate.Date > endDateTempay && PatientStartDate.Status.ToLower() == "billeddate")
                {
                    PatientProgramData patientProgramData1 = patientProgramData as PatientProgramData;
                    if (patientProgramData1 != null)
                    {
                        BillingProcess billingProcess = new BillingProcess();
                        BilledDates billedDates = billingProcess.GetPatientLastBilledPeriods(
                                                                  patientProgramData1.PatienttId,
                                                                  patientProgramData1.PatientProgramid,
                                                                  billingCode.BillingCodeID, con);
                        DateTime startDate = billedDates.StartDate;
                        //startDate = BillingProcess.GetLocalTimeFromUTC((DateTime)startDate, con);
                        DateTime startDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)startDate, con);
                        DateTime Enddate = billedDates.EndDate;
                        // Enddate = BillingProcess.GetLocalTimeFromUTC((DateTime)Enddate, con);
                        DateTime endDatetemp = BillingProcess.GetUTCFromLocalTime((DateTime)Enddate, con);

                        AddBilledData(patientProgramData1, startDateTemp, endDatetemp, startDate,
                                      Enddate, true, (Enddate - startDate).Days);

                    }
                }
                else
                {
                    if (flagFirstCycle)
                    {
                        newStartDate = BillingProcess.GetUTCFromLocalTime((DateTime)newStartDate, con);
                    }
                    DateTime newStartDatetemp = BillingProcess.GetLocalTimeFromUTC((DateTime)newStartDate, con);
                    DateTime newBilledDate = DateTime.UtcNow.Date;
                    DateTime newBilledDateTemp = DateTime.MinValue;
                    if (PatientStartDate.Status.ToLower() == "active")
                    {
                        newBilledDate = RPMCycleBasedBilling.GetNextBilledDate((DateTime)PatientStartDate.StartDate);
                        newBilledDateTemp = newBilledDate;// BillingProcess.GetLocalTimeFromUTC((DateTime)newBilledDate, con);
                    }
                    else
                    {
                        newBilledDate = newStartDate.AddMonths(1);
                        newBilledDate = new DateTime(newBilledDate.Year, newBilledDate.Month, newStartDate.Day - 1);
                        newBilledDateTemp = newBilledDate;// BillingProcess.GetLocalTimeFromUTC((DateTime)newBilledDate, con);
                    }
                    DateTime todayDate = DateTime.UtcNow;
                    todayDate = BillingProcess.GetLocalTimeFromUTC((DateTime)todayDate, con);
                    bool bCycleCompleted = newBilledDateTemp.Date <= todayDate.Date ? true : false;
                    int nDays = Math.Abs((todayDate.Date - newStartDate.Date).Days);
                    //if (nDays+1 ==billingCode.Frequency)
                    //{
                    //    bCycleCompleted=true;
                    //}
                    //if (nDays > 0)
                    //    nDays =nDays+1;
                    AddBilledData((PatientProgramData)patientProgramData, newStartDate, newBilledDate, newStartDatetemp, newBilledDateTemp, bCycleCompleted, nDays);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
            }
        }
        private void AddBilledData(PatientProgramData patientProgramData, DateTime startDate, DateTime endDate, DateTime stDateTemp, DateTime NextEndDate, bool bCycleComplete, int DaysCompleted)
        {
            DateTime startDateLoc = stDateTemp;
            DateTime enddateLoc = NextEndDate;
            List<Dates> Dates = new List<Dates>();
            for (int i = 0; i<=DaysCompleted; i++)
            {
                if (i==0)
                {
                    NextEndDate = startDateLoc.AddDays(1).Date.AddSeconds(-1);

                }
                else if (i==DaysCompleted)
                {
                    stDateTemp = startDateLoc.AddDays(i).Date;
                    NextEndDate = stDateTemp.AddDays(1).Date.AddSeconds(-1);
                }
                else
                {
                    stDateTemp = startDateLoc.AddDays(i).Date;
                    NextEndDate = stDateTemp.AddDays(1).Date.AddSeconds(-1);

                }

                stDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)stDateTemp, con);
                NextEndDate = BillingProcess.GetUTCFromLocalTime((DateTime)NextEndDate, con);


                Dates.Add(new Dates() { StartDate = (DateTime)stDateTemp, EndDate = NextEndDate, Totalreading=0 });

            }
            List<PatientInteraction> PatientInteractiontimList = new List<PatientInteraction>();
            if (Dates.Count>0)
            {

                foreach (Dates date in Dates)
                {

                    List<PatientInteraction> PatientInteractiontim = billing.GetPatientInteractiontime(patientProgramData, con, date.StartDate, date.EndDate).ToList();
                    //List<VitalReading> VitalReadings = billing.GetVitalReadings(patientProgramData, con).Where(s => s.ReadingDate >= date.StartDate && s.ReadingDate <= date.EndDate).ToList();

                    if (PatientInteractiontim != null && PatientInteractiontim.Count>0)
                    {
                        foreach (var list in PatientInteractiontim)
                        {
                            PatientInteractiontimList.Add(list);
                        }

                    }

                }


            }





            //List<PatientInteraction> PatientInteractiontim = billing.GetPatientInteractiontime(patientProgramData, con).Where(s => s.Date >= stDateTemp && s.Date <= NextEndDate).ToList();
            //var isEstablishedExist = PatientInteractiontimList.Where(s => s.IsCallNote == 1).ToList();
            //if (isEstablishedExist.Count() > 0)
            //{
            //    isEstablishedExist = isEstablishedExist.Where(s => s.IsEstablishedCall == 1).ToList();
            //    if (isEstablishedExist != null && isEstablishedExist.Count() == 0)
            //    {
            //        PatientInteractiontimList.Clear();
            //    }
            //}
            int TotalReading = PatientInteractiontimList.Sum(s => s.Duration);
            if (PatientInteractiontimList.Count==0 || PatientInteractiontimList==null /*|| isEstablishedExist.Count==0*/)
            {
                TotalReading = 0;
            }

            if (TotalReading >= billingCode.TargetReadings * 60)
            {
                TotalReading = billingCode.TargetReadings * 60;
            }
            startDateLoc = BillingProcess.GetUTCFromLocalTime((DateTime)startDateLoc, con);
            SetData(patientProgramData, TotalReading, Convert.ToDateTime(startDateLoc), billingCode.BillingCodeID, DaysCompleted+1, bCycleComplete, (DateTime?)endDate.Date);
            if (bCycleComplete)
            {
                PatientDailyBillingData patientDailyBillingData = new PatientDailyBillingData()
                {
                    PatientId = ((PatientProgramData)patientProgramData).PatienttId,
                    PatientProgramId = ((PatientProgramData)patientProgramData).PatientProgramid,
                    BillingCodeId = billingCode.BillingCodeID,
                    Status = ((PatientProgramData)patientProgramData).Status,
                    TotalVitalCount = 0,
                    TotalDuration = TotalReading,
                    StartDate = startDateLoc,
                    DaysCompleted = DaysCompleted,
                    LastBilledDate = (DateTime?)endDate,
                    CreatedOn = DateTime.UtcNow
                };
                RPMCycleBasedBilling RPMCycleBasedBilling = new RPMCycleBasedBilling();
                RPMCycleBasedBilling.UpdatePatientBilledData(patientDailyBillingData, billingCode, stDateTemp, con);
            }
        }
        public void SetData(object PatientProgramDatas, int totalReadings, DateTime? startDate,
                            int billingCodeId, int daysCompleted, bool CycleCompleted, DateTime? LastBilledDate)
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

