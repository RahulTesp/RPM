using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Reflection;

namespace RPMPatientBilling.PatientBilling
{
    public class RPMCycleBasedBilling
    {
        enum DayConditions
        {
            Today,
            isPast,
            isFuture,

        };
        enum MonthConditions
        {

            isLastMonth,
            isCurrentMonth,
        };
        public class taskdata
        {

            public taskdata(ManualResetEvent _manualResetEvent)
            {

                this.manualResetEvent = _manualResetEvent;
            }
            public ManualResetEvent manualResetEvent { get; set; }
        }

        public void GenaratePateintBillingCount(string ConnectionString)
        {
            try
            {
                //Console.WriteLine("Genarate Pateint Billing count For Report - start");
                BillingProcess billing = new BillingProcess();
                List<PatientProgramData> patientProgramData = billing.ActivepatientReader(ConnectionString);
                List<BillingCodes> billingCodes = billing.GetBillingCodeDetails(ConnectionString);
               
                List<ManualResetEvent> evts = new List<ManualResetEvent>();
                int nCount = 1;
                Console.WriteLine(@"TotalSize : " + patientProgramData.Count);
                if (patientProgramData != null && billingCodes != null)
                {
                    foreach (PatientProgramData data in patientProgramData)
                    {
                       
                            string Program = billing.GetProgramName(ConnectionString, data.PatientProgramid);
                            Console.WriteLine(nCount+". Processing Patient Id " + data.PatienttId);
                            IBilling ibilling;
                            BillingCountTaskData td = new BillingCountTaskData(null, data);

                            if (Program == "RPM")
                            {
                                ibilling = new CPT99453(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99454(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99457(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99458(ConnectionString);
                                ibilling.Execute(td);
                            }
                            else if (Program == "CCM-C")
                            {
                                //G0506 is for Complex CCM so commenting below lines
                                //ibilling = new CPTG0506(ConnectionString);
                                //ibilling.Execute(td);
                                ibilling = new CPT99490(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99439(ConnectionString);
                                ibilling.Execute(td);

                            }
                            else if (Program == "CCM-P")
                            {
                                //G0506 is for Complex CCM so commenting below lines
                                //ibilling = new CPTG0506(ConnectionString);
                                //ibilling.Execute(td);

                                ibilling = new CPT99491(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99437(ConnectionString);
                                ibilling.Execute(td);

                            }
                            else if (Program == "C-CCM")
                            {
                                ibilling = new CPTG0506(ConnectionString);
                                ibilling.Execute(td);

                                ibilling = new CPT99487(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99489(ConnectionString);
                                ibilling.Execute(td);
                            }
                            else if (Program == "PCM-P")
                            {
                                ibilling = new CPT99424(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99425(ConnectionString);
                                ibilling.Execute(td);


                            }
                            else if (Program == "PCM-C")
                            {

                                ibilling = new CPT99426(ConnectionString);
                                ibilling.Execute(td);
                                ibilling = new CPT99427(ConnectionString);
                                ibilling.Execute(td);

                            }
                            
                        

                        
                        nCount++;
                       /* foreach (var code in billingCodes)
                        {
                            ManualResetEvent me = new ManualResetEvent(false);
                            BillingCountTaskData td = new BillingCountTaskData(me, data);
                            evts.Add(me);
                            switch (code.BillingCode)
                            {
                                case "99453":
                                    ibilling = new CPT99453(ConnectionString);
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td);
                                    break;
                                case "99454":
                                    ibilling = new CPT99454(ConnectionString);
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td);
                                    break;
                                case "99457":
                                    ibilling = new CPT99457(ConnectionString);
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td);
                                    break;
                                case "99458":
                                    ibilling = new CPT99458(ConnectionString);
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td);
                                    break;
                            }
                            if (evts.Count >= 64)
                            {
                                Console.WriteLine("Waiting all tasks to complete..." + "Batch - " + nCount + " " + DateTime.UtcNow.ToString());
                                if (evts.Count > 0) ManualResetEvent.WaitAll(evts.ToArray());
                                evts.Clear();
                                nCount++;
                                Console.WriteLine("All tasks completed..." + DateTime.UtcNow.ToString());
                            }
                        }*/
                    }
                    //Console.WriteLine("Waiting all tasks to complete..." + "Batch - " + nCount + " " + DateTime.UtcNow.ToString());
                   // if (evts.Count > 0) ManualResetEvent.WaitAll(evts.ToArray());
                  //  Console.WriteLine("All tasks completed..." + DateTime.UtcNow.ToString());

                }
                Console.WriteLine("Thread 1 Completed - Genarate Pateint Billing count For Report");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return;
            }
        }
        public void GetPatientBillingReport(string ConnectionString)
        {
            try
            {
                List<ManualResetEvent> events = new List<ManualResetEvent>();
                Console.WriteLine("Thread 1 Started - GenaratePateintBilling  resuls For Report");
                BillingProcess billing = new BillingProcess();
                List<BillingCodes> billingCodes = billing.GetBillingCodeDetails(ConnectionString);
                IBillingResult ibilling;
                foreach (var code in billingCodes)
                {
                    switch (code.BillingCode)
                    {
                        case "99453":
                            ManualResetEvent evt = new ManualResetEvent(false);
                            taskdata td = new taskdata(evt);
                            ibilling = new BCPT99453(ConnectionString);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td);
                            events.Add(evt);
                            break;
                        case "99454":
                            ManualResetEvent evt1 = new ManualResetEvent(false);
                            taskdata td1 = new taskdata(evt1);
                            ibilling = new BCPT99454(ConnectionString);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td1);
                            events.Add(evt1);
                            break;
                        case "99457":
                            ManualResetEvent evt2 = new ManualResetEvent(false);
                            taskdata td2 = new taskdata(evt2);
                            ibilling = new BCPT99457(ConnectionString);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td2);
                            events.Add(evt2);
                            break;
                        case "99458":
                            ManualResetEvent evt3 = new ManualResetEvent(false);
                            taskdata td3 = new taskdata(evt3);
                            ibilling = new BCPT99458(ConnectionString);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ibilling.Execute), td3);
                            events.Add(evt3);
                            break;
                    }

                }
                WaitHandle.WaitAll(events.ToArray());
                Console.WriteLine("Thread 1 Completed - GenaratePateintBilling  resuls For Report");



            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return;
            }
        }
        public void UpdatePatientBilling(string ConnectionString)
        {
            try
            {
                BillingProcess billing = new BillingProcess();
                List<PatientProgramData> patientProgramData = billing.ActivepatientReader(ConnectionString);
                if (patientProgramData == null) return;
                List<BillingCodes> billingCodes = billing.GetBillingCodeDetails(ConnectionString);
                if (billingCodes == null) return;
                foreach (PatientProgramData pd in patientProgramData)
                {
                    BillingProcess bp = new BillingProcess();
                    List<PatientDailyBillingData> lstDailyData = bp.GetPatientBillingCounts(pd.PatienttId, pd.PatientProgramid, ConnectionString);
                    if (lstDailyData == null) continue;
                    foreach (PatientDailyBillingData pdb in lstDailyData)
                    {
                            BillingCodes bc = billingCodes.Find(y => y.BillingCodeID == pdb.BillingCodeId);
                            if (bc != null)
                            {

                                string programName = billing.GetProgramName(ConnectionString, pdb.PatientProgramId);
                                // PatientStartDate psd = null;
                                switch (bc.BillingCode)
                                {
                                    case "99453":
                                        if (pdb.StartDate == null) continue;
                                        if (pdb.Status.ToLower().Equals("active") ||
                                           pdb.Status.ToLower().Equals("onhold") ||
                                           pdb.Status.ToLower().Equals("readytodischarge")||
                                           pdb.Status.ToLower().Equals("inactive"))
                                        {
                                            AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        }
                                        break;
                                    case "G0506":
                                        if (pdb.StartDate == null) continue;
                                        if (pdb.Status.ToLower().Equals("active") ||
                                           pdb.Status.ToLower().Equals("onhold") ||
                                           pdb.Status.ToLower().Equals("readytodischarge")||
                                           pdb.Status.ToLower().Equals("inactive"))
                                        {
                                            AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        }
                                        break;
                                        /*case "99454":
                                            if (pdb.StartDate == null) continue;
                                            if (pdb.Status.ToLower().Equals("active") ||
                                               pdb.Status.ToLower().Equals("onhold") ||
                                               pdb.Status.ToLower().Equals("readytodischarge"))
                                            {
                                                AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                            }
                                            break;
                                        case "99457":
                                            if (pdb.StartDate == null) continue;
                                            if (pdb.Status.ToLower().Equals("active") ||
                                               pdb.Status.ToLower().Equals("onhold") ||
                                               pdb.Status.ToLower().Equals("readytodischarge"))
                                            {
                                                AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                            }
                                            break;
                                        case "99458":
                                            if (pdb.StartDate == null) continue;
                                            if (pdb.Status.ToLower().Equals("active") ||
                                               pdb.Status.ToLower().Equals("onhold") ||
                                               pdb.Status.ToLower().Equals("readytodischarge"))
                                            {
                                                BillingCodes bc1 = billingCodes.Find(y => y.BillingCodeID == 3); //457 billing code.
                                                AddData(pdb, bc1, (DateTime)pdb.StartDate, ConnectionString);
                                            }
                                            break;*/
                                }


                            }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public void UpdatePatientBilledData(PatientDailyBillingData pdb,
                                             BillingCodes bc,
                                             DateTime startDate,
                                             string ConnectionString)
        {
            AddData(pdb, bc, startDate, ConnectionString);
        }
        //private void AddData(PatientDailyBillingData pdb,
        //                     BillingCodes bc,
        //                     DateTime startDate,
        //                     string ConnectionString)
        //{
        //    int TotalReading = pdb.TotalVitalCount;
        //    int TargetReading = bc.TargetReadings;
        //    if (bc.BillingCode.Equals("99457") || bc.BillingCode.Equals("99458")) 
        //    {
        //        TargetReading = TargetReading * 60;
        //        TotalReading = pdb.TotalDuration;
        //    }
        //    BillingProcess billing = new BillingProcess();
        //    if (pdb.DaysCompleted >= bc.BillingThreshold &&
        //        TotalReading >= TargetReading)
        //    {
        //        BillingData bd = pdb.ToBillingData();
        //        if(bc.BillingCode.Equals("99453"))
        //        { 
        //            bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
        //        }              

        //        bd.TargetMet = true;
        //        bd.ReadyToBill = true;
        //        bd.BilledDuration = TargetReading;
        //        billing.SavePatientBillingResult(bd, ConnectionString);
        //       // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : True & TM: true");
        //    }
        //    else if (pdb.DaysCompleted < bc.BillingThreshold &&
        //            TotalReading >= TargetReading)
        //    {
        //        BillingData bd = pdb.ToBillingData();
        //        bd.BilledDate = null;
        //        bd.TargetMet = false;
        //        bd.ReadyToBill = false;
        //        bd.BilledDuration = TargetReading;
        //        billing.SavePatientBillingResult(bd, ConnectionString);
        //       // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : True & TM: false");
        //    }
        //    else if (pdb.DaysCompleted >= bc.BillingThreshold &&
        //             TotalReading < TargetReading)
        //    {
        //        BillingData bd = pdb.ToBillingData();
        //        if (bc.BillingCode.Equals("99453"))
        //        {
        //            bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
        //        }
        //        bd.TargetMet = false;
        //        bd.ReadyToBill = false;
        //        bd.BilledDuration = TargetReading;
        //        billing.SavePatientBillingResult(bd, ConnectionString);
        //       // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : false & TM: false");
        //    }
        //    else
        //    {
        //        BillingData bd = pdb.ToBillingData();
        //        if (bc.BillingCode.Equals("99453"))
        //        {
        //            bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
        //        }
        //        bd.TargetMet = false;
        //        bd.ReadyToBill = false;
        //        bd.BilledDuration = TargetReading;
        //        billing.SavePatientBillingResult(bd, ConnectionString);

        //        // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " Not ready for bill");

        //    }

        //}
        private void AddData(PatientDailyBillingData pdb,
                             BillingCodes bc,
                             DateTime startDate,
                             string ConnectionString)
        {
            int TotalReading = pdb.TotalVitalCount;
            int TargetReading = bc.TargetReadings;
            if (bc.BillingCode.Equals("99457") || bc.BillingCode.Equals("99458") ||bc.BillingCode.Equals("99490") ||bc.BillingCode.Equals("99439")||bc.BillingCode.Equals("99491") ||bc.BillingCode.Equals("99437")||bc.BillingCode.Equals("99487") ||bc.BillingCode.Equals("99489")||bc.BillingCode.Equals("99424") ||bc.BillingCode.Equals("99425")||bc.BillingCode.Equals("99426") ||bc.BillingCode.Equals("99427"))
            {
                TargetReading = TargetReading * 60;
                TotalReading = pdb.TotalDuration;
            }
            BillingProcess billing = new BillingProcess();
            if (pdb.DaysCompleted >= bc.BillingThreshold &&
                TotalReading >= TargetReading)
            {
                BillingData bd = pdb.ToBillingData();
                if (bc.BillingCode.Equals("99453"))
                {
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
                }
                if(bc.BillingCode.Equals("G0506"))
                {
                    bd.BilledDate = startDate.Date;
                }

                bd.TargetMet = true;
                bd.ReadyToBill = true;
                bd.BilledDuration = TargetReading;
                billing.SavePatientBillingResult(bd, ConnectionString);
                // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : True & TM: true");
            }
            //Patient billing table record insert before billed date can be removed - removed by rahul

            //else if (pdb.DaysCompleted < bc.BillingThreshold &&
            //        TotalReading >= TargetReading)
            //{
            //    BillingData bd = pdb.ToBillingData();
            //    if (bc.BillingCode.Equals("99453"))
            //    {
            //        bd.TargetMet = false;
            //        bd.BilledDate = null;
            //        //bd.TargetMet = true;
            //        bd.ReadyToBill = false;
            //        bd.BilledDuration = TargetReading;
            //        billing.SavePatientBillingResult(bd, ConnectionString);
            //    }
            //    else
            //    {
            //        bd.BilledDate = null;
            //        bd.TargetMet = true;
            //        bd.ReadyToBill = false;
            //        bd.BilledDuration = TargetReading;
            //        billing.SavePatientBillingResult(bd, ConnectionString);
            //    }


            //    // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : True & TM: false");
            //}
            else if (pdb.DaysCompleted >= bc.BillingThreshold &&
                     TotalReading < TargetReading)
            {
                BillingData bd = pdb.ToBillingData();
                if (bc.BillingCode.Equals("99453"))
                {
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
                }
                bd.TargetMet = false;
                bd.ReadyToBill = false;
                bd.BilledDuration = TargetReading;
                billing.SavePatientBillingResult(bd, ConnectionString);
                // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : false & TM: false");
            }
            //else
            //{
            //    BillingData bd = pdb.ToBillingData();
            //    bd.TargetMet = false;
            //    bd.ReadyToBill = false;
            //    bd.BilledDuration = TargetReading;
            //    billing.SavePatientBillingResult(bd, ConnectionString);

            //    // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " Not ready for bill");

            //}

        }
        public static DateTime GetNextBilledDate(DateTime startDate)
        {
            DateTime nextDate = startDate.AddMonths(1);
            if (startDate.Day >= 1 && startDate.Day <= 15)
            {
                return  new DateTime(nextDate.Year, nextDate.Month, 1);
            }
            else
            {
                return  new DateTime(nextDate.Year, nextDate.Month, 16);
            }
        }
    }
}
