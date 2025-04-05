using RPMPatientBilling.Model;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace RPMPatientBilling.PatientBilling
{
    public class BillingProcess
    {        
        public List<PatientProgramData> ActivepatientReader(string connectionString)
        {
            try
            {
               // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientProgramInfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientId", 0);
                        command.Parameters.AddWithValue("@patientProgramId", 0);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientProgramData> patientDataList = new List<PatientProgramData>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientProgramData patientData = new PatientProgramData();
                            patientData.PatienttId = Convert.ToInt32(reader["PatientId"]);
                            patientData.PatientProgramid = Convert.ToInt32(reader["PatientProgramId"]);
                            patientData.ActiveDate = Convert.ToDateTime(reader["CreatedOn"]);
                            patientData.TargetReading = Convert.ToInt32(reader["TargetReadings"]);
                            patientData.Status = Convert.ToString(reader["Status"]);
                            patientDataList.Add(patientData);
                        }
                        
                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return patientDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;    
            }



        }
        public string GetProgramName(string connectionString,int PatientProgramId)
        {
            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetProgramName", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        string ProgramName = string.Empty;
                       
                        while (reader.Read())
                        {
                            ProgramName = Convert.ToString(reader["ProgramName"]);
                        }

                        if (ProgramName == string.Empty)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return ProgramName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }

        public List<PatientProgramDatas> ActivepatientReaderForUI(string connectionString)
        {
            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientProgramInfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                       
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientProgramDatas> patientDataList = new List<PatientProgramDatas>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientProgramDatas patientData = new PatientProgramDatas();
                            patientData.PatienttId = Convert.ToInt32(reader["PatientId"]);
                            patientData.PatientProgramid = Convert.ToInt32(reader["PatientProgramId"]);
                            patientData.ActiveDate = Convert.ToDateTime(reader["CreatedOn"]);
                            patientData.TargetReading = Convert.ToInt32(reader["TargetReadings"]);
                            patientData.status = Convert.ToString(reader["Status"]);
                            patientDataList.Add(patientData);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return patientDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<BillingCodesData> GetBillingCodeDetailsForUI(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingCodeDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<BillingCodesData> billingCodesList = new List<BillingCodesData>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            BillingCodesData BillingCodes = new BillingCodesData();
                            BillingCodes.BillingCodeID = Convert.ToInt32(reader["Id"]);
                            BillingCodes.BillingCode = Convert.ToString(reader["BillingCode"]);
                            BillingCodes.Frequency = Convert.ToInt32(reader["Frequency"]);
                            BillingCodes.FrequencyPeriod = Convert.ToString(reader["FrequencyPeriod"]);
                            BillingCodes.BillingThreshold = Convert.ToInt32(reader["BillingThreshold"]);
                            BillingCodes.BillingPeriod = Convert.ToString(reader["BillingPeriod"]);
                            BillingCodes.TargetReadings = Convert.ToInt32(reader["TargetReadings"]);
                            billingCodesList.Add(BillingCodes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return billingCodesList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<VitalReadings> GetVitalReadings(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalReadingsDashBoard", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<VitalReadings> VitalReadingList = new List<VitalReadings>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            VitalReadings VitalReading = new VitalReadings();
                            VitalReading.ReadingDate = Convert.ToDateTime(reader["ReadingDate"]);
                            VitalReading.Totalreadings = Convert.ToInt32(reader["Totalreadings"]);
                            VitalReading.programId = Convert.ToInt32(reader["PatientProgramID"]);

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
        public List<PatientInteractionUI> GetPatientInteractiontimeForUI(string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallOrReviewTimeDashBoard", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientInteractionUI> PatientInteractiontime = new List<PatientInteractionUI>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientInteractionUI PatientInteractiontimes = new PatientInteractionUI();
                            PatientInteractiontimes.Duration = Convert.ToInt32(reader["Duration"]);
                            PatientInteractiontimes.Date = Convert.ToDateTime(reader["Date"]);
                            PatientInteractiontimes.IsCallNote = Convert.ToInt32(reader["IsCallNote"]);
                            PatientInteractiontimes.IsEstablishedCall = Convert.ToInt32(reader["IsEstablishedCall"]);
                            PatientInteractiontimes.PatientProgramId = Convert.ToInt32(reader["PatientProgramID"]);
                            PatientInteractiontime.Add(PatientInteractiontimes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientInteractiontime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<BillingCodes> GetBillingCodeDetails(string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingCodeDetails", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<BillingCodes> billingCodesList = new List<BillingCodes>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            BillingCodes BillingCodes = new BillingCodes();
                            BillingCodes.BillingCodeID = Convert.ToInt32(reader["Id"]);
                            BillingCodes.BillingCode = Convert.ToString(reader["BillingCode"]);
                            BillingCodes.Frequency = Convert.ToInt32(reader["Frequency"]);
                            BillingCodes.FrequencyPeriod = Convert.ToString(reader["FrequencyPeriod"]);
                            BillingCodes.BillingThreshold = Convert.ToInt32(reader["BillingThreshold"]);
                            BillingCodes.BillingPeriod = Convert.ToString(reader["BillingPeriod"]);
                            BillingCodes.TargetReadings = Convert.ToInt32(reader["TargetReadings"]);
                            billingCodesList.Add(BillingCodes);
                        }
                        
                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return billingCodesList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<VitalReading> GetVitalReadings(object patientProgramData, string connectionString)
        {

            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetVitalReadings", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
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
        //public List<PatientInteraction> GetPatientInteractiontime(object patientProgramData, string connectionString)
        //{
        //    try
        //    {
        //        //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();

        //            using (SqlCommand command = new SqlCommand("usp_GetPatientCallOrReviewTime", con))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
        //                command.Parameters.AddWithValue("@patientId", ((PatientProgramData)patientProgramData).PatienttId);
        //                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
        //                returnParameter.Direction = ParameterDirection.ReturnValue;
        //                SqlDataReader reader = command.ExecuteReader();

        //                List<PatientInteraction> PatientInteractiontime = new List<PatientInteraction>();
        //                List<int> roleids = new List<int>();
        //                while (reader.Read())
        //                {
        //                    PatientInteraction PatientInteractiontimes = new PatientInteraction();
        //                    PatientInteractiontimes.Duration = Convert.ToInt32(reader["Duration"]);
        //                    PatientInteractiontimes.Date = Convert.ToDateTime(reader["Date"]);
        //                    PatientInteractiontimes.IsCallNote = Convert.ToInt32(reader["IsCallNote"]);
        //                    PatientInteractiontimes.IsEstablishedCall = Convert.ToInt32(reader["IsEstablishedCall"]);
        //                    PatientInteractiontime.Add(PatientInteractiontimes);
        //                }

        //                if (reader.FieldCount == 0)
        //                {
        //                    con.Close();
        //                    return null;
        //                }
        //                con.Close();
        //                return PatientInteractiontime;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
        //        return null;
        //    }



        //}
        public List<PatientInteraction> GetPatientInteractiontime(object patientProgramData, string connectionString, DateTime startDate, DateTime enddate)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientCallOrReviewTimeLocal", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
                        command.Parameters.AddWithValue("@patientId", ((PatientProgramData)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", enddate);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientInteraction> PatientInteractiontime = new List<PatientInteraction>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientInteraction PatientInteractiontimes = new PatientInteraction();
                            PatientInteractiontimes.Duration = Convert.ToInt32(reader["Duration"]);
                            PatientInteractiontimes.Date = Convert.ToDateTime(reader["Date"]);
                            PatientInteractiontimes.IsCallNote = Convert.ToInt32(reader["IsCallNote"]);
                            PatientInteractiontimes.IsEstablishedCall = Convert.ToInt32(reader["IsEstablishedCall"]);
                            PatientInteractiontime.Add(PatientInteractiontimes);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientInteractiontime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public int GetPatientSmsCount(object patientProgramData, string connectionString, DateTime startDate, DateTime enddate)
        {
            try
            {
                int count;  
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();


                    using (SqlCommand command = new SqlCommand("usp_GetPatientSmsCount", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientId", ((PatientProgramData)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@startdate", startDate);
                        command.Parameters.AddWithValue("@enddate", enddate);
                        //count = (int)command.ExecuteScalar();
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            return 1;
                            
                        }
                        con.Close();
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return 0;
            }



        }
        public PatientStartDate GetBillingStartDate(object patientProgramData,int billingCodeId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetBillingStartDates", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", ((PatientProgramData)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@PatientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientStartDate PatientStartDate = new PatientStartDate();
                        while (reader.Read())
                        {
                            if (DBNull.Value.Equals(reader["BillingStartDate"]))
                            {
                                PatientStartDate.StartDate = null;
                            }
                            else
                            {
                                PatientStartDate.StartDate = Convert.ToDateTime(reader["BillingStartDate"]);
                            }
                            
                            PatientStartDate.Status = Convert.ToString(reader["Status"]);
                        }
                        con.Close();
                        return PatientStartDate;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientStartDate GetPatientBillingStartDate(object patientProgramData, int billingCodeId,string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingStartDate", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", ((PatientProgramData)patientProgramData).PatienttId);
                        command.Parameters.AddWithValue("@PatientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
                        command.Parameters.AddWithValue("@BillingCodeId", billingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientStartDate PatientStartDate = new PatientStartDate();
                        while (reader.Read())
                        {
                            if (DBNull.Value.Equals(reader["BillingStartDate"]))
                            {
                                PatientStartDate.StartDate = null;
                            }
                            else
                            {
                                PatientStartDate.StartDate = Convert.ToDateTime(reader["BillingStartDate"]);
                            }
                            
                            PatientStartDate.Status = Convert.ToString(reader["Status"]);
                        }
                            
                        con.Close();
                        return PatientStartDate;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public void SavePatientBillingData(PatientDailyBillingData Info, string connectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientsDailyBillingCount", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@patientId", Info.PatientId);
                    command.Parameters.AddWithValue("@patientProgramId", Info.PatientProgramId);
                    command.Parameters.AddWithValue("@billingCodeid", Info.BillingCodeId);
                    command.Parameters.AddWithValue("@status", Info.Status);
                    command.Parameters.AddWithValue("@totalVitalCount", Info.TotalVitalCount);
                    command.Parameters.AddWithValue("@totalDuration", Info.TotalDuration);
                    if(Info.StartDate != null)
                    {
                        command.Parameters.AddWithValue("@startDate", Info.StartDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@startDate",DBNull.Value);
                    }

                    if (Info.LastBilledDate != null)
                    {
                        command.Parameters.AddWithValue("@lastbilleddate", Info.LastBilledDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@lastbilleddate", DBNull.Value);
                    }

                    command.Parameters.AddWithValue("@daysCompleted", Info.DaysCompleted);

                    command.Parameters.AddWithValue("@createdOn", Info.CreatedOn);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    con.Close();
                    
                }
            }
            catch(Exception ex)
            {
                
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                
            }
        }
        public void SavePatientBillingResult(BillingData Info, string connectionString)
        {
            try
            {
                if (Info.StartDate == null) return;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientBilling", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", Info.PatientProgramId);
                    command.Parameters.AddWithValue("@BillingCodeId", Info.BillingCodeId);
                    command.Parameters.AddWithValue("@StartDate", Info.StartDate);
                    if(Info.BilledDate!=null)
                        command.Parameters.AddWithValue("@BilledDate", Info.BilledDate);
                    else
                        command.Parameters.AddWithValue("@BilledDate", DBNull.Value);
                    command.Parameters.AddWithValue("@TotalReadings", Info.Totalreadings);
                    command.Parameters.AddWithValue("@TargetMet", Info.TargetMet ? 1 : 0);
                    command.Parameters.AddWithValue("@ReadyToBill", Info.ReadyToBill ? 1 : 0);
                    command.Parameters.AddWithValue("@BilledReading", Info.BilledDuration);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    con.Close();

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");

            }
        }
        public void ClearPatientreportdetails(string connectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_TruncatePatientReportdetails", con);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    con.Close();

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");

            }
        }
        public void SavePatientBillingDataUI(BillingDataUI Info, string connectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("Ins_PatientReportdetails", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", Info.PatientProgramId);
                    command.Parameters.AddWithValue("@BillingCodeId", Info.BillingCodeId);
                    command.Parameters.AddWithValue("@BillingCode", Info.BillingCode);
                    command.Parameters.AddWithValue("@TotalReading", Info.Totalreadings);
                    command.Parameters.AddWithValue("@TargetReading", Info.Targetreading);
                    command.Parameters.AddWithValue("@TargetMet", Info.TargetMet ? 1 : 0);
                    command.Parameters.AddWithValue("@ReadyToBill", Info.ReadyToBill ? 1 : 0);
                    command.Parameters.AddWithValue("@OnHold", Info.OnHold ? 1 : 0);
                    command.Parameters.AddWithValue("@MissingInfo", Info.MissingInfo ? 1 : 0);
                    command.Parameters.AddWithValue("@IsToday", Info.IsToday ? 1 : 0);
                    command.Parameters.AddWithValue("@IsPast", Info.IsPast ? 1 : 0);
                    command.Parameters.AddWithValue("@IsFuture", Info.IsFuture ? 1 : 0);
                    command.Parameters.AddWithValue("@IsCurrentMonth", Info.IsCurrentMonth ? 1 : 0);
                    command.Parameters.AddWithValue("@IsLastMonth", Info.IsLastMonth ? 1 : 0);
                    command.Parameters.AddWithValue("@StartDate", Info.StartDate);
                    command.Parameters.AddWithValue("@EndDate", Info.BilledDate);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    con.Close();

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");

            }
        }
        public List<PatientDailyBillingData> GetPatientBillingCounts(int BillingCodeId,string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingCounts", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@billingCodeId", BillingCodeId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientDailyBillingData> PatientDailyBillingDataList = new List<PatientDailyBillingData>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientDailyBillingData PatientDailyBillingData = new PatientDailyBillingData();
                            PatientDailyBillingData.PatientId = Convert.ToInt32(reader["PatientId"]);
                            PatientDailyBillingData.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                            PatientDailyBillingData.BillingCodeId = Convert.ToInt32(reader["BillingCodeId"]);
                            PatientDailyBillingData.TotalVitalCount = Convert.ToInt32(reader["TotalVitalCount"]);
                            PatientDailyBillingData.TotalDuration = Convert.ToInt32(reader["TotalDuration"]);
                            if (DBNull.Value.Equals(reader["StartDate"]))
                            {
                                PatientDailyBillingData.StartDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.StartDate = Convert.ToDateTime(reader["StartDate"]);
                            }
                            
                            if(DBNull.Value.Equals(reader["LastBilledDate"]))
                            {
                                PatientDailyBillingData.LastBilledDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.LastBilledDate = Convert.ToDateTime(reader["LastBilledDate"]);
                            }
                            //PatientDailyBillingData.LastBilledDate = (DBNull.Value.Equals(reader["LastBilledDate"])) ? null: Convert.ToDateTime(reader["LastBilledDate"]); ;
                            PatientDailyBillingData.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            PatientDailyBillingDataList.Add(PatientDailyBillingData);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientDailyBillingDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public PatientProgramData GetPatientDetails(int patientId,int patientprogramId,string connectionString)
        {

            try
            {
                // string ConnectionString = ConfigurationManager.AppSettings["RPM"].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientProgramInfo", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@patientId", patientId);
                        command.Parameters.AddWithValue("@patientProgramId", patientprogramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        PatientProgramData patientData = new PatientProgramData();

                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            
                            patientData.PatienttId = Convert.ToInt32(reader["PatientId"]);
                            patientData.PatientProgramid = Convert.ToInt32(reader["PatientProgramId"]);
                            patientData.ActiveDate = Convert.ToDateTime(reader["CreatedOn"]);
                            patientData.TargetReading = Convert.ToInt32(reader["TargetReadings"]);
                            patientData.Status = Convert.ToString(reader["Status"]);

                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return patientData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public List<PatientDailyBillingData> GetPatientBillingCounts(int patientId, int patientPgmId, string connectionString)
        {
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientBillingCountsBypatient", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientPrigramId", patientPgmId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        List<PatientDailyBillingData> PatientDailyBillingDataList = new List<PatientDailyBillingData>();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientDailyBillingData PatientDailyBillingData = new PatientDailyBillingData();
                            PatientDailyBillingData.PatientId = Convert.ToInt32(reader["PatientId"]);
                            PatientDailyBillingData.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                            PatientDailyBillingData.BillingCodeId = Convert.ToInt32(reader["BillingCodeId"]);
                            PatientDailyBillingData.Status = reader["Status"].ToString();
                            PatientDailyBillingData.TotalVitalCount = Convert.ToInt32(reader["TotalVitalCount"]);
                            PatientDailyBillingData.TotalDuration = Convert.ToInt32(reader["TotalDuration"]);
                            PatientDailyBillingData.DaysCompleted = Convert.ToInt32(reader["DaysCompleted"]);
                            if (DBNull.Value.Equals(reader["StartDate"]))
                            {
                                PatientDailyBillingData.StartDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.StartDate = Convert.ToDateTime(reader["StartDate"]);
                            }

                            if (DBNull.Value.Equals(reader["LastBilledDate"]))
                            {
                                PatientDailyBillingData.LastBilledDate = null;
                            }
                            else
                            {
                                PatientDailyBillingData.LastBilledDate = Convert.ToDateTime(reader["LastBilledDate"]);
                            }
                            //PatientDailyBillingData.LastBilledDate = (DBNull.Value.Equals(reader["LastBilledDate"])) ? null: Convert.ToDateTime(reader["LastBilledDate"]); ;
                            PatientDailyBillingData.CreatedOn = Convert.ToDateTime(reader["CreatedOn"]);
                            PatientDailyBillingDataList.Add(PatientDailyBillingData);
                        }

                        if (reader.FieldCount == 0)
                        {
                            con.Close();
                            return null;
                        }
                        con.Close();
                        return PatientDailyBillingDataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }
        }
        public BilledDates GetPatientLastBilledPeriods(int patientId, int patientPgmId,int billingcode,string connectionString)
        {
            BilledDates ret = new BilledDates();
            try
            {
                //string ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetLastBilledDates", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);
                        command.Parameters.AddWithValue("@PatientProgramId", patientPgmId); 
                        command.Parameters.AddWithValue("@BillingCodeId", billingcode);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            ret.StartDate = Convert.ToDateTime(reader["LastBilledStartDate"]);
                            ret.EndDate = Convert.ToDateTime(reader["LastBilledEndDate"]);
                        } 
                        con.Close();
                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured - class Name :" + this.GetType().Name + ",  Method Name : " + MethodBase.GetCurrentMethod().Name + ", Error Message :" + ex.Message + "");
                return null;
            }



        }
        public static DateTime GetLocalTimeFromUTC(DateTime dt, string con)
        {
            List<SystemConfigInfo> configData = BillingProcess.GetSystemConfig(con, "Provider", String.Empty);
            if (configData == null || configData.Count <= 0)
            {
                throw new Exception("Provider time zone not set.");
            }
            SystemConfigInfo tz = configData.Find(x => x.Name == "TimeZone");
            if (tz == null) throw new Exception("Provider time zone not set.");

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz.Value);
            DateTime newUtc = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZoneInfo);
            return newUtc;
        }

        public static DateTime GetUTCFromLocalTime(DateTime dt, string con)
        {
            List<SystemConfigInfo> configData = BillingProcess.GetSystemConfig(con, "Provider", String.Empty);
            if (configData == null || configData.Count <= 0)
            {
                throw new Exception("Provider time zone not set.");
            }
            SystemConfigInfo tz = configData.Find(x => x.Name == "TimeZone");
            if (tz == null) throw new Exception("Provider time zone not set.");

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz.Value);
            DateTime newUtc = TimeZoneInfo.ConvertTimeToUtc(dt, timeZoneInfo);
            return newUtc;
        }

        public List<VitalReading> GetVitalReadingsLocal(object patientProgramData, string connectionString, DateTime startDate, DateTime endDate)
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
                        command.Parameters.AddWithValue("@patientProgramId", ((PatientProgramData)patientProgramData).PatientProgramid);
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
        public static List<SystemConfigInfo> GetSystemConfig(string cs, string category, string createdby)
        {
            List<SystemConfigInfo> ret = new List<SystemConfigInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(cs))
                {
                    //string query = "select * from SystemConfigurations where Category='iGlucose'";
                    SqlCommand command = new SqlCommand("usp_GetSystemConfig", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Category", SqlDbType.NVarChar).Value = category;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = createdby;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SystemConfigInfo info = new SystemConfigInfo();
                        info.Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader["name"].ToString() : string.Empty;
                        info.Value = !reader.IsDBNull(reader.GetOrdinal("Value")) ? reader["Value"].ToString() : string.Empty;
                        info.Descripiton = !reader.IsDBNull(reader.GetOrdinal("Description")) ? reader["Description"].ToString() : string.Empty;
                        ret.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
    }
}
