using System.Data.SqlClient;
using RPMPatientBilling.Interface;
using RPMPatientBilling.Model;
using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Linq;


namespace RPMPatientBilling.PatientBilling
{
    public class RPMDaysBasedBilling
    {
        

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
                    }
                   
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
                                        //case "99454":
                                        //    if (pdb.StartDate == null) continue;
                                        //    if (pdb.Status.ToLower().Equals("active") ||
                                        //       pdb.Status.ToLower().Equals("onhold") ||
                                        //       pdb.Status.ToLower().Equals("readytodischarge")||
                                        //       pdb.Status.ToLower().Equals("inactive"))
                                        //    {
                                        //        AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        //    }
                                        //    break;
                                        //case "99457":
                                        //if (pdb.StartDate == null) continue;
                                        //if (pdb.Status.ToLower().Equals("active") ||
                                        //   pdb.Status.ToLower().Equals("onhold") ||
                                        //   pdb.Status.ToLower().Equals("readytodischarge")||
                                        //   pdb.Status.ToLower().Equals("inactive"))
                                        //{
                                        //    AddData(pdb, bc, (DateTime)pdb.StartDate, ConnectionString);
                                        //}
                                        //break;
                                        //case "99458":
                                        //    if (pdb.StartDate == null) continue;
                                        //    if (pdb.Status.ToLower().Equals("active") ||
                                        //       pdb.Status.ToLower().Equals("onhold") ||
                                        //       pdb.Status.ToLower().Equals("readytodischarge")||
                                        //       pdb.Status.ToLower().Equals("inactive"))
                                        //    {
                                        //        BillingCodes bc1 = billingCodes.Find(y => y.BillingCodeID == 3); //457 billing code.
                                        //        AddData(pdb, bc1, (DateTime)pdb.StartDate, ConnectionString);
                                        //    }
                                        //    break;
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

                    using (SqlCommand command = new SqlCommand("usp_GetpatientbillingbybillingCodeid_DaysBasedBilling", con))
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
                if (bc.BillingCode.Equals("99457") || bc.BillingCode.Equals("99458")||bc.BillingCode.Equals("99490") ||bc.BillingCode.Equals("99439")||bc.BillingCode.Equals("99491") ||bc.BillingCode.Equals("99437")||bc.BillingCode.Equals("99487") ||bc.BillingCode.Equals("99489")||bc.BillingCode.Equals("99424") ||bc.BillingCode.Equals("99425")||bc.BillingCode.Equals("99426") ||bc.BillingCode.Equals("99427"))
                {
                    TargetReading = TargetReading * 60;
                    TotalReading = pdb.TotalDuration;
                }
                BillingProcess billing = new BillingProcess();
                if (pdb.DaysCompleted >= bc.BillingThreshold &&
                    TotalReading >= TargetReading)
                {
                    BillingData bd = pdb.ToBillingData();
                    if (bc.BillingCode.Equals("G0506"))
                    {
                        bd.BilledDate = startDate.Date;
                    }
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold-1);
                    bd.TargetMet = true;
                    bd.ReadyToBill = true;
                    bd.BilledDuration = TargetReading;
                    billing.SavePatientBillingResult(bd, ConnectionString);
                    
                }
                
                else if (pdb.DaysCompleted >= bc.BillingThreshold &&
                         TotalReading < TargetReading)
                {
                    BillingData bd = pdb.ToBillingData();
                    bd.BilledDate = startDate.AddDays(bc.BillingThreshold - 1);
                    bd.TargetMet = false;
                    bd.ReadyToBill = false;
                    bd.BilledDuration = TargetReading;
                    billing.SavePatientBillingResult(bd, ConnectionString);
                   
                }
                
            }

        }
        public void UpdatePatientBilledData(PatientDailyBillingData pdb,
                                             BillingCodes bc,
                                             DateTime startDate,
                                             string ConnectionString)
        {
            AddData(pdb, bc, startDate, ConnectionString);
        }
    }
}
