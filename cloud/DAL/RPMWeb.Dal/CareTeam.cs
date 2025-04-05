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
    public sealed class CareTeam
    {
        public int AddCareTeam(CareTeams data, string ConnectionString)
        {
           
            try
            {
                string CareTeamMembersInput = null;
                if (data.MemberUserId != null)
                {
                    string CareTeamMembers = "Insert into CareTeamMembers(UserId, CareTeamId, CreatedBy)values";
                    string inserts = string.Empty;
                    List<int> membersIds = data.MemberUserId;
                    foreach (int memberId in membersIds)
                    {
                        string insertvalues = "(" + memberId + ",'@@CareTeamId','" + data.CreatedBy + "'),";
                        inserts = inserts + insertvalues;
                    }
                    string script = CareTeamMembers + inserts;
                     CareTeamMembersInput = script.Substring(0, script.Length - 1);
                }               
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsCareTeams", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", data.Name);
                    command.Parameters.AddWithValue("@ManagerID", data.ManagerId);
                    command.Parameters.AddWithValue("@CareTeamMembers", CareTeamMembersInput);
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (!id.Equals(0))
                    {
                        return id;
                    }
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }     
        public int UpdateCareTeam(CareTeams data, string ConnectionString)
            {
            bool ret = true;
            try
            {
                string CareTeamMembersInput = null;
                if (data.MemberUserId != null)
                {
                    string CareTeamMembers = "Insert into @@CareTeamMembers(UserId, CareTeamId, CreatedBy)values";
                    string inserts = string.Empty;
                    List<int> membersIds = data.MemberUserId;
                    foreach (int memberId in membersIds)
                    {
                        string insertvalues = "(" + memberId + ",'" + data.careTeamId + "','" + data.CreatedBy + "'),";
                        inserts = inserts + insertvalues;
                    }
                    string script = CareTeamMembers + inserts;
                    if (inserts != "")
                    {
                        CareTeamMembersInput = script.Substring(0, script.Length - 1);
                    }
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdCareTeams", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@careTeamId", data.careTeamId);
                    command.Parameters.AddWithValue("@Name", data.Name);
                    command.Parameters.AddWithValue("@ManagerID", data.ManagerId);
                    command.Parameters.AddWithValue("@CareTeamMembers", CareTeamMembersInput);
                    command.Parameters.AddWithValue("@ModifiedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    
                    return id;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<NonAssignedCM> GetNonAssignedCaerTeamMembers(string CreatedBy, string ConnectionString)
        {
            List<NonAssignedCM> Listinfo = new List<NonAssignedCM>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetNonAssignedCareTeamMembers", connection);
                    command.CommandType = CommandType.StoredProcedure;                   
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        NonAssignedCM info = new NonAssignedCM();
                        info.CareTeamMemberUserId = (!DBNull.Value.Equals(reader["CareTeamMemberUserId"])) ? Convert.ToInt32(reader["CareTeamMemberUserId"]) : 0;
                        info.UserName = reader["UserName"].ToString();
                        info.Name = reader["Name"].ToString();
                        Listinfo.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;

        }
        public DataSet GetTeamTasks( DateTime TodayDate,DateTime StartDate,DateTime EndDate, int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetTeamTasks", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@TodayDate", TodayDate);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);                      
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
        public DataSet GetTasks(string CreatedBy, string ConnectionString)
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
        public DataSet GetTeamAlerts(int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetTeamAlerts", con))
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
        public DataSet GetDetailedTeamAlerts(int CareTeamId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetDetailedTeamAlerts", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CareTeamId", @CareTeamId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
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
        public CareTeamInfo GetTeamMembersByTeam(int CareTeamId, string CreatedBy, string ConnectionString)
        {
            CareTeamInfo Listinfo = new CareTeamInfo();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetCareTeamById", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@TeamId", SqlDbType.Int).Value = CareTeamId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {                        
                        string teamName =  (!DBNull.Value.Equals(reader["Name"])) ? reader["Name"].ToString() : string.Empty;
                        if (string.IsNullOrEmpty(teamName)) continue;
                        Listinfo.TeamId = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        Listinfo.TeamName = teamName;
                        Listinfo.ManagerUserId = (!DBNull.Value.Equals(reader["ManagerUserId"])) ? Convert.ToInt32(reader["ManagerUserId"]) :0;
                        Listinfo.ManagerFirstName = (!DBNull.Value.Equals(reader["ManagerFirstName"])) ? reader["ManagerFirstName"].ToString() : string.Empty;
                        Listinfo.ManagerLastName = (!DBNull.Value.Equals(reader["ManagerLastName"])) ? reader["ManagerLastName"].ToString() : string.Empty;
                        Listinfo.ManagerPatientCount= (!DBNull.Value.Equals(reader["ManagerPatientCount"])) ? Convert.ToInt32(reader["ManagerPatientCount"]) : 0;
                        CareTeamMember tm = new CareTeamMember();
                        int temp= (!DBNull.Value.Equals(reader["UserId"])) ? Convert.ToInt32(reader["UserId"]) : 0;
                        if (temp != 0)
                        {
                            tm.UserId = temp;
                            tm.MemberFirstName = (!DBNull.Value.Equals(reader["FirstName"])) ? reader["FirstName"].ToString() : string.Empty;
                            tm.MemberLastName = (!DBNull.Value.Equals(reader["LastName"])) ? reader["LastName"].ToString() : string.Empty;
                            tm.MemberPatientCount = (!DBNull.Value.Equals(reader["UserPatientCount"])) ? Convert.ToInt32(reader["UserPatientCount"]) : 0;
                            tm.MemberDischargePatientCount = (!DBNull.Value.Equals(reader["UserPatientCount"])) ? Convert.ToInt32(reader["DischargePatientCount"]) : 0;
                            tm.Role = (!DBNull.Value.Equals(reader["RoleName"])) ? reader["RoleName"].ToString() : string.Empty;
                            Listinfo.TeamMembers.Add(tm);
                        }                       
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;
        }
        public List<CareTeamBaseInfo> GetTeam(string CreatedBy, string ConnectionString)
        {
            List<CareTeamBaseInfo> Listinfo = new List<CareTeamBaseInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetCareTeams", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CareTeamBaseInfo tm = new CareTeamBaseInfo();
                        tm.TeamId = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        tm.TeamName = (!DBNull.Value.Equals(reader["Name"])) ? reader["Name"].ToString() : string.Empty;
                        tm.ManagerUserId = (!DBNull.Value.Equals(reader["ManagerID"])) ? Convert.ToInt32(reader["ManagerID"]) : 0;
                        Listinfo.Add(tm);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;
        }
        public List<PatientCareTeamMembers> GetPatientCareteamMembers(string CreatedBy, string ConnectionString)
        {
            List<PatientCareTeamMembers> Listinfo = new List<PatientCareTeamMembers>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientCareTeamMembers", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PatientCareTeamMembers ptm = new PatientCareTeamMembers();
                       
                        ptm.MemberUserName = (!DBNull.Value.Equals(reader["UserName"])) ? reader["UserName"].ToString() : string.Empty;
                        ptm.MemberName = (!DBNull.Value.Equals(reader["Name"])) ? reader["Name"].ToString() : string.Empty;
                        ptm.Role = (!DBNull.Value.Equals(reader["Role"])) ? reader["Role"].ToString() : string.Empty;
                        Listinfo.Add(ptm);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;
        }
    }
}
