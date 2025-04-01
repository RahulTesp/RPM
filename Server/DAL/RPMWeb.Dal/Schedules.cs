using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPMWeb.Dal
{
    class Schedules
    {
        public int AddSchedule(ScheduleInfo info, string ConnectionString)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                 
                    SqlCommand command = new SqlCommand("usp_InsCareTeamSchedule", connection);
                    command.CommandType = CommandType.StoredProcedure;                   
                    command.Parameters.Add("@Schedule", SqlDbType.VarChar).Value = info.Schedule;
                    command.Parameters.Add("@ScheduleTypeId", SqlDbType.Int).Value = info.ScheduleTypeId;
                    command.Parameters.Add("@Mon", SqlDbType.Bit).Value = info.Mon;
                    command.Parameters.Add("@Tue", SqlDbType.Bit).Value = info.Tue;
                    command.Parameters.Add("@Wed", SqlDbType.Bit).Value = info.Wed;
                    command.Parameters.Add("@Thu", SqlDbType.Bit).Value = info.Thu;
                    command.Parameters.Add("@Fri", SqlDbType.Bit).Value = info.Fri;
                    command.Parameters.Add("@Sat", SqlDbType.Bit).Value = info.Sat;
                    command.Parameters.Add("@Sun", SqlDbType.Bit).Value = info.Sun;
                    command.Parameters.Add("@WeekSelection", SqlDbType.TinyInt).Value = info.WeekSelection;
                    command.Parameters.Add("@StartDate", SqlDbType.Date).Value = info.StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.Date).Value = info.EndDate;
                    command.Parameters.Add("@StartTime", SqlDbType.Time).Value = info.StartTime;
                    command.Parameters.Add("@Duration", SqlDbType.SmallInt).Value = info.Duration;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = info.Comments;
                    command.Parameters.Add("@AssignedTo", SqlDbType.Int).Value = info.AssignedTo;
                    command.Parameters.Add("@AssignedBy", SqlDbType.Int).Value = info.AssignedBy;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return 0;
                    }
                    else
                    {
                        return id;
                    }

                }
            }
            catch
            {
                throw;
            }

        }
        public bool UpdateSchedule(ScheduleInfo info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdCareTeamSchedule", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = info.Id;
                    command.Parameters.Add("@Schedule", SqlDbType.VarChar).Value = info.Schedule;
                    command.Parameters.Add("@ScheduleTypeId", SqlDbType.Int).Value = info.ScheduleTypeId;
                    command.Parameters.Add("@Mon", SqlDbType.Bit).Value = info.Mon;
                    command.Parameters.Add("@Tue", SqlDbType.Bit).Value = info.Tue;
                    command.Parameters.Add("@Wed", SqlDbType.Bit).Value = info.Wed;
                    command.Parameters.Add("@Thu", SqlDbType.Bit).Value = info.Thu;
                    command.Parameters.Add("@Fri", SqlDbType.Bit).Value = info.Fri;
                    command.Parameters.Add("@Sat", SqlDbType.Bit).Value = info.Sat;
                    command.Parameters.Add("@Sun", SqlDbType.Bit).Value = info.Sun;
                    command.Parameters.Add("@WeekSelection", SqlDbType.TinyInt).Value = info.WeekSelection;
                    command.Parameters.Add("@StartDate", SqlDbType.Date).Value = info.StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.Date).Value = info.EndDate;
                    command.Parameters.Add("@StartTime", SqlDbType.Time).Value = info.StartTime;
                    command.Parameters.Add("@Duration", SqlDbType.SmallInt).Value = info.Duration;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = info.Comments;
                    command.Parameters.Add("@AssignedTo", SqlDbType.Int).Value = info.AssignedTo;
                    command.Parameters.Add("@AssignedBy", SqlDbType.Int).Value = info.AssignedBy;
                    command.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = info.CreatedBy;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public bool UpdateCurrentSchedule(CurrentScheduleInfo info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdCareTeamScheduleDetail", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CurrentScheduleId", SqlDbType.Int).Value = info.CurrentScheduleId;                    
                    command.Parameters.Add("@ScheduleDate", SqlDbType.Date).Value = info.ScheduleDate;
                    command.Parameters.Add("@StartTime", SqlDbType.Time).Value = info.StartTime;
                    command.Parameters.Add("@Duration", SqlDbType.SmallInt).Value = info.Duration;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = info.Comments;               
                    command.Parameters.Add("@IsCompleted", SqlDbType.Bit).Value = info.IsCompleted;
                    command.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = info.CreatedBy;                   
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public bool UpdateScheduleCompletion(CompletedSchedules data, string ConnectionString)
        {
            bool ret = true;
            try
            {               
                string ScheduleIds = "insert into @@#ScheduleIds(Id)values";
                string ScheduleIdsinserts = string.Empty;
                List<int> ids = data.IDs;
                foreach (int Id in ids)
                {
                    string insertvalues = "('" + Id + "'),";
                    ScheduleIdsinserts = ScheduleIdsinserts + insertvalues;
                }
                string script = ScheduleIds + ScheduleIdsinserts;
                string CompletedSchedulesIds = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdScheduleStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;                  
                    command.Parameters.Add("@Ids", SqlDbType.NVarChar).Value = CompletedSchedulesIds;
                    command.Parameters.AddWithValue("@ModifiedBy", data.ModifiedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }

        public DataSet GetMasterDataForSchedules(int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForCareTeamSchedules", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "ScheduleTypes";
                                ds.Tables[1].TableName = "PatientOrContactName";
                                ds.Tables[2].TableName = "AssigneeList";
                            }
                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetSchedules> GetCareTeamSchedule(DateTime StartDate,DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<GetSchedules> getSchedules = new List<GetSchedules>();
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetWorklistCareTeamSchedule", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = StartDate;
                        command.Parameters.Add("@EndDate", SqlDbType.Date).Value = EndDate;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                DateTime[] dates = ds.Tables[0].AsEnumerable().Select(s => s.Field<DateTime>("Date")).Distinct().ToArray<DateTime>();                                
                                foreach (DateTime dt in dates)
                                {
                                    GetSchedules schedules = new GetSchedules();
                                    List<SchedueInfo> schedueInfos = new List<SchedueInfo>();
                                    schedules.ScheduleDate = dt;
                                    foreach(DataRow dr in ds.Tables[0].Rows)
                                    {
                                        if(dt== Convert.ToDateTime(dr["Date"]).Date)
                                        {
                                            SchedueInfo schedueInfo = new SchedueInfo();
                                            schedueInfo.Id = Convert.ToInt32(dr["Id"]);
                                            schedueInfo.CurrentScheduleId = (!DBNull.Value.Equals(dr["CurrentScheduleId"])) ? Convert.ToInt32(dr["CurrentScheduleId"]) : 0;
                                            schedueInfo.PatientId = (!DBNull.Value.Equals(dr["PatientId"])) ? Convert.ToInt32(dr["PatientId"]) : 0;
                                            schedueInfo.PatientProgramId = (!DBNull.Value.Equals(dr["PatientProgramId"])) ? Convert.ToInt32(dr["PatientProgramId"]) : 0;
                                            schedueInfo.ProgramName = dr["ProgramName"].ToString();
                                            schedueInfo.ScheduleTime= dr["Time"].ToString();
                                            schedueInfo.ScheduleType= dr["ScheduleType"].ToString();
                                            schedueInfo.Description= dr["Description"].ToString();
                                            schedueInfo.ContactName= dr["ContactName"].ToString();
                                            schedueInfo.AssignedBy = (!DBNull.Value.Equals(dr["AssignedBy"])) ? Convert.ToInt32(dr["AssignedBy"]) : 0;
                                            schedueInfo.AssignedByName= dr["AssignedByName"].ToString();
                                            schedueInfo.IsCompleted = (!DBNull.Value.Equals(dr["IsCompleted"])) ? Convert.ToBoolean(dr["IsCompleted"]) : false;
                                            schedueInfos.Add(schedueInfo);
                                        }
                                    }
                                    schedules.SchedueInfos = schedueInfos;
                                    getSchedules.Add(schedules);
                                }
                            }
                        }
                    }
                }
                return getSchedules;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ScheduleInfo GetWorklistScheduleById(int CurrentScheduleId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetWorklistSchedulesById", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;                      
                        command.Parameters.AddWithValue("@CurrentScheduleId", CurrentScheduleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        ScheduleInfo info = new ScheduleInfo();
                        while (reader.Read())
                        {
                            info.Id= (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                            info.CurrentScheduleId = (!DBNull.Value.Equals(reader["CurrentScheduleId"])) ? Convert.ToInt32(reader["CurrentScheduleId"]) : 0;
                            info.IsPatient = (!DBNull.Value.Equals(reader["IsPatient"])) ? Convert.ToBoolean(reader["IsPatient"]) : false;
                            info.Schedule= reader["Schedule"].ToString();
                            info.ScheduleTypeId = (!DBNull.Value.Equals(reader["ScheduleTypeId"])) ? Convert.ToInt32(reader["ScheduleTypeId"]) : 0;
                            info.Mon = (!DBNull.Value.Equals(reader["Mon"])) ? Convert.ToBoolean(reader["Mon"]) : false;
                            info.Tue = (!DBNull.Value.Equals(reader["Tue"])) ? Convert.ToBoolean(reader["Tue"]) : false;
                            info.Wed = (!DBNull.Value.Equals(reader["Wed"])) ? Convert.ToBoolean(reader["Wed"]) : false;
                            info.Thu = (!DBNull.Value.Equals(reader["Thu"])) ? Convert.ToBoolean(reader["Thu"]) : false;
                            info.Fri = (!DBNull.Value.Equals(reader["Fri"])) ? Convert.ToBoolean(reader["Fri"]) : false;
                            info.Sat = (!DBNull.Value.Equals(reader["Sat"])) ? Convert.ToBoolean(reader["Sat"]) : false;
                            info.Sun = (!DBNull.Value.Equals(reader["Sun"])) ? Convert.ToBoolean(reader["Sun"]) : false;
                            info.WeekSelection = (!DBNull.Value.Equals(reader["WeekSelection"])) ? Convert.ToInt32(reader["WeekSelection"]) : 0;
                            info.StartDate = (!DBNull.Value.Equals(reader["StartDate"])) ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                            info.EndDate = (!DBNull.Value.Equals(reader["EndDate"])) ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;
                            info.StartTime = reader["StartTime"].ToString();
                            info.CurrentScheduleDate= (!DBNull.Value.Equals(reader["CurrentScheduleDate"])) ? Convert.ToDateTime(reader["CurrentScheduleDate"]) : DateTime.MinValue;
                            info.CurrentScheduleStartTime = reader["CurrentScheduleStartTime"].ToString();
                            info.Duration = (!DBNull.Value.Equals(reader["Duration"])) ? Convert.ToInt32(reader["Duration"]) : 0;
                            info.CurrentScheduleDuration = (!DBNull.Value.Equals(reader["CurrentScheduleDuration"])) ? Convert.ToInt32(reader["CurrentScheduleDuration"]) : 0;
                            info.Comments= reader["Comments"].ToString();
                            info.CurrentScheduleComments = reader["CurrentScheduleComments"].ToString();
                            info.AssignedTo= (!DBNull.Value.Equals(reader["AssignedTo"])) ? Convert.ToInt32(reader["AssignedTo"]) : 0;
                            info.AssigneeName= reader["AssigneeName"].ToString();
                            info.AssignedBy = (!DBNull.Value.Equals(reader["AssignedBy"])) ? Convert.ToInt32(reader["AssignedBy"]) : 0;
                            info.CreatedBy = reader["CreatedBy"].ToString();
                            info.IsCompleted= (!DBNull.Value.Equals(reader["IsCompleted"])) ? Convert.ToBoolean(reader["IsCompleted"]) : false;
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Schedule GetToDoList(string Username, DateTime day, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientSchedule", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", Username);
                        command.Parameters.AddWithValue("@StartDate", day);
                        command.Parameters.AddWithValue("@EndDate", day);

                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        Schedule ScheduleList = new Schedule();
                        //ScheduleTypes types = new ScheduleTypes();
                        ScheduleList.ScheduleTypes = new List<ScheduleTypes>();
                        //List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            ScheduleTypes types = new ScheduleTypes();
                            types.ScheduleType = Convert.ToString(reader["ScheduleType"]);
                            types.Time = Convert.ToString(reader["Time"]);
                            types.Decription = Convert.ToString(reader["Description"]);
                            ScheduleList.ScheduleTypes.Add(types);


                        }

                        /*if (ScheduleList.ScheduleTypes.Count == 0)
                        {
                            return null;
                        }*/
                        return ScheduleList;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
