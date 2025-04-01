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
    class Alerts
    {
        public DataSet GetAlerts(int RoleId,string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetWorkListAlerts", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "Summary";
                                ds.Tables[1].TableName = "Details";
                                ds.Tables[2].TableName = "AlertTypes";
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
        public DataSet GetTeamAlertsById(string AlertType, int CareTeamId,int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetTeamAlertsByIdAndByAlertType", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@AlertType", AlertType);
                        command.Parameters.AddWithValue("@CareTeamId", CareTeamId);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
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
        public GetAlert GetAlertById(int Id, string ConnectionString)
        {
            GetAlert info = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetWorkListAlertById", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        info = new GetAlert();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.AlertType = reader["AlertType"].ToString();
                        info.Description = reader["Description"].ToString();
                        info.DueDate = (!DBNull.Value.Equals(reader["DueDate"])) ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue;
                        info.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                        info.Priority = reader["Priority"].ToString();
                        info.PriorityId = (!DBNull.Value.Equals(reader["PriorityId"])) ? Convert.ToInt32(reader["PriorityId"]) : 0;
                        info.Status = reader["Status"].ToString();
                        info.AssignedMember = reader["AssignedMember"].ToString();
                        info.AssignedMemberId= (!DBNull.Value.Equals(reader["AssignedMemberId"])) ? Convert.ToInt32(reader["AssignedMemberId"]) : 0;
                        info.CareTeamId= (!DBNull.Value.Equals(reader["CareTeamId"])) ? Convert.ToInt32(reader["CareTeamId"]) : 0;
                        info.Comments = reader["Comments"].ToString();
                    }
                    connection.Close();
                    if(info != null)
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
        public GetAlert GetAlertByIdPatient(int Id, string ConnectionString)
        {
            GetAlert info = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetWorkListAlertByIdPatient", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        info = new GetAlert();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.AlertType = reader["AlertType"].ToString();
                        info.Description = reader["Description"].ToString();
                        info.DueDate = (!DBNull.Value.Equals(reader["DueDate"])) ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue;
                        info.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                        info.Priority = reader["Priority"].ToString();
                        info.PriorityId = (!DBNull.Value.Equals(reader["PriorityId"])) ? Convert.ToInt32(reader["PriorityId"]) : 0;
                        info.Status = reader["Status"].ToString();
                        info.AssignedMember = reader["AssignedMember"].ToString();
                        info.AssignedMemberId = (!DBNull.Value.Equals(reader["AssignedMemberId"])) ? Convert.ToInt32(reader["AssignedMemberId"]) : 0;
                        info.CareTeamId = (!DBNull.Value.Equals(reader["CareTeamId"])) ? Convert.ToInt32(reader["CareTeamId"]) : 0;
                        info.Comments = reader["Comments"].ToString();
                    }
                }
            }
            catch
            {
                throw;
            }
            return info;

        }
        public int AlertResponse(TaskResponse info, string ConnectionString)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdAlertSaveResponse", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@AlertId", SqlDbType.Int).Value = info.AlertId;
                    command.Parameters.Add("@RoleId", SqlDbType.Int).Value = info.RoleId;
                    command.Parameters.Add("@AlertStatus", SqlDbType.NVarChar).Value = info.AlertStatus;
                    command.Parameters.Add("@AssigneeId", SqlDbType.Int).Value = info.UserId;
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
        public int AlertResponseFromPatient(TaskResponse info, string ConnectionString)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdAlertSaveResponseFromPatient", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@AlertId", SqlDbType.Int).Value = info.AlertId;
                    command.Parameters.Add("@RoleId", SqlDbType.Int).Value = info.RoleId;
                    command.Parameters.Add("@AlertStatus", SqlDbType.NVarChar).Value = info.AlertStatus;
                    command.Parameters.Add("@AssigneeId", SqlDbType.Int).Value = info.UserId;
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
    }
}
