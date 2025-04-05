using Microsoft.Data.SqlClient;
using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System.Data;
using System.Reflection;


namespace RPMPatientBilling.PatientBilling
{
    public class RPMBilling
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
                Console.WriteLine("Genarate Pateint Billing count For Report - start");
                BillingProcess billing = new BillingProcess();
                List<PatientProgramData> patientProgramData = billing.ActivepatientReader(ConnectionString);
                List<BillingCodes> billingCodes = billing.GetBillingCodeDetails(ConnectionString);
                List<ManualResetEvent> evts = new List<ManualResetEvent>();
                //int nCount = 1;
                int nCount = 1;
                Console.WriteLine(@"TotalSize : " + patientProgramData.Count);
                if (patientProgramData != null && billingCodes != null)
                {
                    foreach (PatientProgramData data in patientProgramData)
                    {
                        
                            Console.WriteLine(nCount+". Processing Patient Id " + data.PatienttId);
                            IBilling ibilling;
                            ibilling = new CPT99453(ConnectionString);
                            BillingCountTaskData td = new BillingCountTaskData(null, data);
                            ibilling.Execute(td);
                            ibilling = new CPT99454(ConnectionString);
                            ibilling.Execute(td);
                            ibilling = new CPT99457(ConnectionString);
                            ibilling.Execute(td);
                            ibilling = new CPT99458(ConnectionString);
                            ibilling.Execute(td);
                            nCount++;





                        //ibilling = new CPT99454(ConnectionString);
                        //ibilling.Execute(td);
                        //ibilling = new CPT99457(ConnectionString);
                        //ibilling.Execute(td);
                        //ibilling = new CPT99458(ConnectionString);
                        //ibilling.Execute(td);

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
                    Console.WriteLine("All tasks completed..." + DateTime.UtcNow.ToString());

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
                                    case "99454":
                                        if (pdb.StartDate == null) continue;
                                        if (pdb.Status.ToLower().Equals("active") ||
                                           pdb.Status.ToLower().Equals("onhold") ||
                                           pdb.Status.ToLower().Equals("readytodischarge")||
                                           pdb.Status.ToLower().Equals("inactive"))
                                        {
                                            AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        }
                                        break;
                                    case "99457":
                                        if (pdb.StartDate == null) continue;
                                        if (pdb.Status.ToLower().Equals("active") ||
                                           pdb.Status.ToLower().Equals("onhold") ||
                                           pdb.Status.ToLower().Equals("readytodischarge")||
                                           pdb.Status.ToLower().Equals("inactive"))
                                        {
                                            AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        }
                                        break;
                                    case "99458":
                                        if (pdb.StartDate == null) continue;
                                        if (pdb.Status.ToLower().Equals("active") ||
                                           pdb.Status.ToLower().Equals("onhold") ||
                                           pdb.Status.ToLower().Equals("readytodischarge")||
                                           pdb.Status.ToLower().Equals("inactive"))
                                        {
                                            BillingCodes bc1 = billingCodes.Find(y => y.BillingCodeID == 3); //457 billing code.
                                            AddData(pdb, bc1, (DateTime)pdb.StartDate, ConnectionString);
                                        }
                                        break;
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
        public List<VitalReading> GetVitalReadingsLocal(int PatientProgramid, string connectionString, DateTime startDate, DateTime endDate)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalReadingsLocal", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", PatientProgramid);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", endDate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<VitalReading> VitalReadingList = new List<VitalReading>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            VitalReading VitalReading = new VitalReading();
                            VitalReading.ReadingDate = Convert.ToDateTime(reader["ReadingDate"]);
                            VitalReading.Totalreadings = Convert.ToInt32(reader["Totalreadings"]);

                            VitalReadingList.Add(VitalReading);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return VitalReadingList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public string GetpatientbillingData(int PatientProgramid,int patientId,DateTime startdate, string connectionString,int billingcodeid)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetpatientbillingbybillingCodeid", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", PatientProgramid);
                        command.Parameters.AddWithValue("@patientId", patientId);
                        command.Parameters.AddWithValue("@billingCodeId", billingcodeid);
                        command.Parameters.AddWithValue("@startdate", startdate);
                       
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<int> roleids = new List<int>();
                        string response = "false";
                        while (reader.Read())
                        {
                           response = Convert.ToString(reader["Response"]);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public void UpdatePatientBilledData(PatientDailyBillingData pdb,
                                             BillingCodes bc,
                                             DateTime startDate,
                                             string ConnectionString)
        {
            AddData(pdb, bc, startDate, ConnectionString);
        }
        private void AddData(PatientDailyBillingData pdb,
                             BillingCodes bc,
                             DateTime startDate,
                             string ConnectionString)
        {
            int TotalReading = pdb.TotalVitalCount;
            int TargetReading = bc.TargetReadings;


            if(bc.BillingCode.Equals("99453"))
            {
                BillingProcess billings = new BillingProcess();
                if (pdb.TotalVitalCount >= bc.TargetReadings)
                {
                    int DaysCompleted = 0;
                    var Startdate = pdb.StartDate;
                    DateTime StartDateTemp = (DateTime)Startdate;
                    StartDateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)StartDateTemp, ConnectionString);
                    DateTime startDateLoc = StartDateTemp;


                    DateTime Enddate = DateTime.UtcNow;

                    DateTime EnddateTemp = BillingProcess.GetLocalTimeFromUTC((DateTime)Enddate.AddDays(1), ConnectionString);

                    var DateDiffDays = Convert.ToDateTime(StartDateTemp).Date - Enddate.Date;
                    int DaysCompletedtotal = Math.Abs(DateDiffDays.Days);
                    var DateDiff = Convert.ToDateTime(Startdate) - Enddate;
                    DaysCompleted = Math.Abs(DateDiff.Days);
                    //DateTime Enddate = DateTime.UtcNow;
                    DateTime? lastdate = null;
                    List<Dates> Dates = new List<Dates>();
                    List<Dates> DatesNew = new List<Dates>();

                    string response = GetpatientbillingData(pdb.PatientProgramId, pdb.PatientId, Convert.ToDateTime(pdb.StartDate), ConnectionString, pdb.BillingCodeId);
                    if(response!="true")
                    {
                        for (int i = 0; i<=DaysCompleted; i++)
                        {
                            if (i==0)
                            {
                                EnddateTemp = startDateLoc.AddDays(1).Date.AddSeconds(-1);

                            }
                            else if (i==DaysCompleted)
                            {
                                StartDateTemp = startDateLoc.AddDays(i).Date;
                                EnddateTemp = StartDateTemp.AddDays(1).Date.AddSeconds(-1);
                            }
                            else
                            {
                                StartDateTemp = startDateLoc.AddDays(i).Date;
                                EnddateTemp = StartDateTemp.AddDays(1).Date.AddSeconds(-1);

                            }

                            StartDateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)StartDateTemp, ConnectionString);
                            EnddateTemp = BillingProcess.GetUTCFromLocalTime((DateTime)EnddateTemp, ConnectionString);


                            Dates.Add(new Dates() { StartDate = (DateTime)StartDateTemp, EndDate = EnddateTemp, Totalreading=0 });




                        }


                        if (Dates.Count>0)
                        {

                            foreach (Dates date in Dates)
                            {

                                List<VitalReading> VitalReadings = GetVitalReadingsLocal(pdb.PatientProgramId, ConnectionString, date.StartDate, date.EndDate).ToList();

                                if (VitalReadings.Count>0)
                                {

                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=1 });

                                }
                                else
                                {
                                    DatesNew.Add(new Dates() { StartDate = (DateTime)date.StartDate, EndDate = date.EndDate, Totalreading=0 });
                                }
                                int VitalCounts = DatesNew.Where(s => s.Totalreading == 1).Count();
                                if (VitalCounts == TargetReading)
                                {
                                    lastdate= date.StartDate;
                                    break;
                                }

                            }


                        }


                        int VitalCount = DatesNew.Where(s => s.Totalreading == 1).Count();








                        // List<VitalReading> VitalReadings = billings.GetVitalReadingsValue(pdb.PatientProgramId, ConnectionString).Where(s => s.ReadingDate >= pdb.StartDate && s.ReadingDate <= Enddate).OrderBy(s => s.ReadingDate).Take(TargetReading).ToList();
                        if (VitalCount== TargetReading)
                        {
                            //var lastDate = lastdate
                            DateTime Billeddate = Convert.ToDateTime(lastdate);
                            BillingData bd = pdb.ToBillingData();
                            bd.BilledDate = Billeddate;
                            bd.TargetMet = true;
                            bd.ReadyToBill = true;
                            bd.BilledDuration = TargetReading;
                            bd.Totalreadings = bc.TargetReadings;
                            billings.SavePatientBillingResult(bd, ConnectionString);
                        }
                    }
                    // check already billed in patientbiling
                    
                    

                }

            }
            else
            {
                if (bc.BillingCode.Equals("99457") || bc.BillingCode.Equals("99458"))
                {
                    TargetReading = TargetReading * 60;
                    TotalReading = pdb.TotalDuration;
                }
                BillingProcess billing = new BillingProcess();
                if (pdb.DaysCompleted >= bc.BillingThreshold &&
                    TotalReading >= TargetReading)
                {
                    BillingData bd = pdb.ToBillingData();
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold-1);
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
                //    bd.BilledDate = null;
                //    bd.TargetMet = true;
                //    bd.ReadyToBill = false;
                //    bd.BilledDuration = TargetReading;
                //    billing.SavePatientBillingResult(bd, ConnectionString);
                //    // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : True & TM: false");
                //}
                else if (pdb.DaysCompleted >= bc.BillingThreshold &&
                         TotalReading < TargetReading)
                {
                    BillingData bd = pdb.ToBillingData();
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
                    bd.TargetMet = false;
                    bd.ReadyToBill = false;
                    bd.BilledDuration = TargetReading;
                    billing.SavePatientBillingResult(bd, ConnectionString);
                    // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " RB : false & TM: false");
                }
                else
                {
                    // Console.WriteLine("Patiend Id : " + pdb.PatientId + " Billing code: " + bc.BillingCode + " Not ready for bill");

                }
            }

        }
    }
}
