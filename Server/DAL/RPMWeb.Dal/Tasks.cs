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
    class Tasks
    {
        public int AddTask(TaskInfo info,  string ConnectionString)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientTasks", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@TaskTypeId", SqlDbType.SmallInt).Value = info.TaskTypeId;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = info.PatientId;
                    command.Parameters.Add("@CareteamMemberUserId", SqlDbType.Int).Value = info.CareteamMemberUserId;
                    command.Parameters.Add("@DueDate", SqlDbType.SmallDateTime).Value = info.DueDate;
                    command.Parameters.Add("@PriorityId", SqlDbType.TinyInt).Value = info.PriorityId;
                    command.Parameters.Add("@Status", SqlDbType.NVarChar).Value = info.Status;
                    command.Parameters.Add("@WatcherUserId", SqlDbType.Int).Value = info.WatcherUserId;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = info.Comments;
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
        public bool UpdateTask(TaskInfo info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientTasks", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = info.Id;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@TaskTypeId", SqlDbType.SmallInt).Value = info.TaskTypeId;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = info.PatientId;
                    command.Parameters.Add("@CareteamMemberUserId", SqlDbType.Int).Value = info.CareteamMemberUserId;
                    command.Parameters.Add("@DueDate", SqlDbType.SmallDateTime).Value = info.DueDate;
                    command.Parameters.Add("@PriorityId", SqlDbType.TinyInt).Value = info.PriorityId;
                    command.Parameters.Add("@Status", SqlDbType.NVarChar).Value = info.Status;
                    command.Parameters.Add("@WatcherUserId", SqlDbType.Int).Value = info.WatcherUserId;
                    command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = info.Comments;
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
        public DataSet GetTasks(DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetWorklistPatientTasks", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "Summary";
                                ds.Tables[1].TableName = "Details";
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
        public DataSet GetTasksByTypeAndId(string TaskType,int CareTeamId, DateTime TodayDate, DateTime StartDate, DateTime EndDate, int RoleId,string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetTeamTaskByIdAndByTaskType", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@TaskType", TaskType);
                        command.Parameters.AddWithValue("@CareTeamId", CareTeamId);
                        command.Parameters.AddWithValue("@TodayDate", TodayDate);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                           
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "Summary";
                                if (ds.Tables.Count > 1)
                                {
                                    ds.Tables[1].TableName = "Details";
                                }
                                else
                                {
                                    DataTable objDt2 = new DataTable("Details");
                                   
                                    ds.Tables.Add(objDt2);
                                }
                                
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
        public GetTask GetTaskById(int Id, string ConnectionString)
        {
            GetTask info = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetWorkListTaskById", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        info = new GetTask();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.TaskType = reader["TaskType"].ToString();
                        info.TaskTypeId = (!DBNull.Value.Equals(reader["TaskTypeId"])) ? Convert.ToInt32(reader["TaskTypeId"]) : 0;
                        info.Description = reader["Description"].ToString();
                        info.DueDate = (!DBNull.Value.Equals(reader["DueDate"])) ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue;
                        info.Priority = reader["Priority"].ToString();
                        info.PriorityId = (!DBNull.Value.Equals(reader["PriorityId"])) ? Convert.ToInt32(reader["PriorityId"]) : 0;
                        info.Status = reader["Status"].ToString();
                        info.AssignedMember = reader["AssignedMember"].ToString();
                        info.AssignedMemberId = (!DBNull.Value.Equals(reader["AssignedMemberId"])) ? Convert.ToInt32(reader["AssignedMemberId"]) : 0;
                        info.CareTeamId = (!DBNull.Value.Equals(reader["CareTeamId"])) ? Convert.ToInt32(reader["CareTeamId"]) : 0;
                        info.Comments = reader["Comments"].ToString();
                    }
                    connection.Close();
                    if (info != null)
                    {
                        info.Members = new List<TeamMember>();

                        SqlCommand command1 = new SqlCommand("usp_GetTeamMember", connection);
                        command1.CommandType = CommandType.StoredProcedure;
                        command1.Parameters.Add("@careteamId", SqlDbType.Int).Value = info.CareTeamId;
                        connection.Open();
                        SqlDataReader reader1 = command1.ExecuteReader();
                        while (reader1.Read())
                        {
                            TeamMember teamMember = new TeamMember();
                            teamMember.Userid = Convert.ToInt32(reader1["UserId"]);
                            teamMember.Member = Convert.ToString(reader1["FirstName"]) +" "+Convert.ToString(reader1["Lastname"]);
                            info.Members.Add(teamMember);


                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return info;

        }
        public DataSet GetMasterDataForTask(int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForTasks", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "PatientList";
                                ds.Tables[1].TableName = "CareTeamMembersList";
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
    }
}
