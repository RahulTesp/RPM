using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RPMWeb.Dal
{
    public sealed class User
    {
        public GetUserProfiles GetUserProfiles(int UserId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetUserProfiles", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        GetUserProfiles userProfiles = new GetUserProfiles();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            roleids.Add(Convert.ToInt32(reader["RoleId"]));
                            userProfiles.UserId = Convert.ToInt32(reader["UserId"]);
                            userProfiles.UserName = reader["UserName"].ToString();
                            userProfiles.FirstName = reader["FirstName"].ToString();
                            userProfiles.MiddleName = reader["MiddleName"].ToString();
                            userProfiles.LastName = reader["LastName"].ToString();
                            //userProfiles.DOB = Convert.ToDateTime(reader["DOB"]);
                            //userProfiles.Gender = reader["Gender"].ToString();
                            userProfiles.MobileNo = reader["MobileNo"].ToString();
                            //userProfiles.AlternateMobNo = reader["AlternateMobNo"].ToString();
                            userProfiles.Email = reader["Email"].ToString();
                            //userProfiles.Address1 = reader["Address1"].ToString();
                            //userProfiles.Address2 = reader["Address2"].ToString();
                            userProfiles.CityId = (!DBNull.Value.Equals(reader["CityID"]))?Convert.ToInt32(reader["CityID"]):0;
                            userProfiles.StateId = (!DBNull.Value.Equals(reader["StateId"])) ? Convert.ToInt32(reader["StateId"]):0;
                            userProfiles.CountryId = (!DBNull.Value.Equals(reader["CountryId"])) ? Convert.ToInt32(reader["CountryId"]):0;
                            userProfiles.ZipCode = reader["ZipCode"].ToString();
                            userProfiles.OrganizationID = (!DBNull.Value.Equals(reader["OrganizationID"])) ? Convert.ToInt32(reader["OrganizationID"]):0;
                            userProfiles.TimeZoneID = (!DBNull.Value.Equals(reader["TimeZoneID"])) ? Convert.ToInt32(reader["TimeZoneID"]):0;
                            //userProfiles.Picture
                            userProfiles.Status = reader["Status"].ToString();
                            userProfiles.HasPatients = (!DBNull.Value.Equals(reader["HasPatients"])) ? Convert.ToBoolean(reader["HasPatients"]) : false;
                        }
                        userProfiles.RoleIds = roleids;
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return userProfiles;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ProileSummary GetMyProfileAndProgram(string Username, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetProfilAndProgram", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@username", Username);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        ProileSummary userProfiles = new ProileSummary();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            userProfiles.Name = Convert.ToString(reader["Name"]);
                            userProfiles.UserName = reader["UserName"].ToString();
                            userProfiles.ProgramName = reader["ProgramName"].ToString();
                            userProfiles.ProgramType = reader["ProgramType"].ToString();
                            userProfiles.CompletedDuration = reader["CompletedDuration"].ToString();
                            userProfiles.Status = reader["Status"].ToString();
                            userProfiles.CurrentDuration = Convert.ToInt32(reader["CurrentDuration"]);
                            userProfiles.TotalDuration = Convert.ToInt32(reader["PatientTotalDuration"]);

                        }

                        /*if (reader.FieldCount == 0)
                        {
                            return null;
                        }*/
                        return userProfiles;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public GetUserProfiles GetMyProfiles(string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMyProfiles", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;                        
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        GetUserProfiles userProfiles = new GetUserProfiles();
                        List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            roleids.Add(Convert.ToInt32(reader["RoleId"]));
                            userProfiles.UserId = Convert.ToInt32(reader["UserId"]);
                            userProfiles.UserName = reader["UserName"].ToString();
                            userProfiles.FirstName = reader["FirstName"].ToString();
                            userProfiles.MiddleName = reader["MiddleName"].ToString();
                            userProfiles.LastName = reader["LastName"].ToString();      
                            //userProfiles.DOB = (!DBNull.Value.Equals(reader["DOB"])) ? Convert.ToDateTime(reader["DOB"]) :DateTime.MinValue;
                            //userProfiles.Gender = reader["Gender"].ToString();
                            userProfiles.MobileNo = reader["MobileNo"].ToString();
                            //userProfiles.AlternateMobNo = reader["AlternateMobNo"].ToString();
                            userProfiles.Email = reader["Email"].ToString();
                            //userProfiles.Address1 = reader["Address1"].ToString();
                            //userProfiles.Address2 = reader["Address2"].ToString();
                            userProfiles.CityId = (!DBNull.Value.Equals(reader["CityID"])) ? Convert.ToInt32(reader["CityID"]) : 0;
                            userProfiles.StateId = (!DBNull.Value.Equals(reader["StateId"])) ? Convert.ToInt32(reader["StateId"]) : 0;
                            userProfiles.CountryId = (!DBNull.Value.Equals(reader["CountryId"])) ? Convert.ToInt32(reader["CountryId"]) : 0;
                            userProfiles.ZipCode = reader["ZipCode"].ToString();
                            userProfiles.OrganizationID = (!DBNull.Value.Equals(reader["OrganizationID"])) ? Convert.ToInt32(reader["OrganizationID"]) : 0;
                            userProfiles.TimeZoneID = (!DBNull.Value.Equals(reader["TimeZoneID"])) ? Convert.ToInt32(reader["TimeZoneID"]) : 0;
                            //userProfiles.Picture
                            userProfiles.Status = reader["Status"].ToString();
                        }
                        userProfiles.RoleIds = roleids;
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return userProfiles;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public NewUserCredential SaveUsers(UserProfiles Info, string ConnectionString)
        {
            NewUserCredential newUserCredential = new NewUserCredential();
            try
            {
                string UserRoles = "INSERT INTO UserRoles([UserId],[RoleId])VALUES";
                string UserRolesinserts = string.Empty;
                int[] roleIDs = Info.RoleId;
                foreach (int roleID in roleIDs)
                {
                    string insertvalues = "('USERIDXXX','" + roleID + "'),";
                    UserRolesinserts = UserRolesinserts + insertvalues;
                }
                string script = UserRoles + UserRolesinserts;
                string UserRolesInput = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsUsersDetails", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", Info.UserName);
                    command.Parameters.AddWithValue("@UserRoles", UserRolesInput);
                    command.Parameters.AddWithValue("@OrganizationID", Info.OrganizationID);
                    command.Parameters.AddWithValue("@MobileNo", Info.MobileNo);
                    command.Parameters.AddWithValue("@Email", Info.Email);
                    command.Parameters.AddWithValue("@Status", Info.Status);
                    command.Parameters.AddWithValue("@FirstName", Info.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", Info.MiddleName);
                    command.Parameters.AddWithValue("@LastName", Info.LastName);
                    //command.Parameters.AddWithValue("@DOB", Info.DOB);
                    //command.Parameters.AddWithValue("@Gender", Info.Gender);
                    command.Parameters.AddWithValue("@CityID", Info.CityId);
                    command.Parameters.AddWithValue("@StateId", Info.StateId);
                    command.Parameters.AddWithValue("@CountryId", Info.CountryId);
                    command.Parameters.AddWithValue("@ZipCode", Info.ZipCode);
                    command.Parameters.AddWithValue("@TimeZoneID", Info.TimeZoneID);
                    command.Parameters.AddWithValue("@Picture", Info.Picture);
                    command.Parameters.AddWithValue("@CreatedBy", Info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        newUserCredential.UserId = Convert.ToInt32(reader["RoleId"]);
                        newUserCredential.password = Convert.ToString(reader["RandomPassword"]);
                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    connection.Close();
                }
                return newUserCredential;
            }
            catch //(Exception ex)
            {
                throw;
            }
        }
        public bool UpdateUser(UserProfiles ui, string ConnectionString)
        {
            bool ret = true;
            try
            {

                string UserRoles = "INSERT INTO UserRoles([UserId],[RoleId])VALUES";
                string UserRolesinserts = string.Empty;
                int[] roleIDs = ui.RoleId;
                foreach (int roleID in roleIDs)
                {
                    string insertvalues = "('"+ui.Id+"','" + roleID + "'),";
                    UserRolesinserts = UserRolesinserts + insertvalues;
                }
                string script = UserRoles + UserRolesinserts;
                string UserRolesInput = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdUserDetails", connection);
                    command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = ui.Id;
                        command.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = ui.FirstName;
                        command.Parameters.Add("@MiddleName", SqlDbType.NVarChar).Value = ui.MiddleName;
                        command.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = ui.LastName;
                        //command.Parameters.Add("@DOB", SqlDbType.Date).Value = ui.DOB;
                        //command.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = ui.Gender;
                        command.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = ui.OrganizationID;
                        command.Parameters.Add("@MobileNo", SqlDbType.NVarChar).Value = ui.MobileNo;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = ui.Email;
                        command.Parameters.Add("@SupervisorId", SqlDbType.Int).Value = ui.SupervisorId;
                        command.Parameters.Add("@UserRoles", SqlDbType.NVarChar).Value = UserRolesInput;
                        command.Parameters.Add("@Status", SqlDbType.NVarChar).Value = ui.Status;
                        command.Parameters.Add("@CityId", SqlDbType.Int).Value = ui.CityId;
                        command.Parameters.Add("@StateId", SqlDbType.Int).Value = ui.StateId;
                        command.Parameters.Add("@CountryId", SqlDbType.Int).Value = ui.CountryId;
                        command.Parameters.Add("@ZipCode", SqlDbType.NVarChar).Value = ui.ZipCode;
                        command.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = ui.CreatedBy;
                        command.Parameters.Add("@TimeZoneId", SqlDbType.Int).Value = ui.TimeZoneID;
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
            catch //(Exception ex)
            {
                throw;
            }
            return ret;

        }
        public DataSet GetAllUsers(int RoleId, string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllUsersDetails",con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using(ds = new DataSet())
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
        public List<string> GetUserHasCriticalAlers(string ConnectionString)
        {
            List<string> ret = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetUserHasCriticalAlerts", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string userName = !reader.IsDBNull(reader.GetOrdinal("UserName")) ? reader["UserName"].ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(userName))
                        {
                            ret.Add(userName);
                        }
                    }
                }
            }
            catch //(Exception ex)
            {
                throw;
            }
            return ret;

        }
        public NewPatientCredential UpdateUserPassword(ResetUserPW Info, string ConnectionString)
        {
            NewPatientCredential newPatientCredential = new NewPatientCredential();
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdUserPassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", Info.UserId);
                    command.Parameters.AddWithValue("@CreatedBy", Info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        newPatientCredential.PatientId = Convert.ToInt32(reader["UserId"]);
                        newPatientCredential.password = Convert.ToString(reader["RandomPassword"]);
                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    connection.Close();
                }
                return newPatientCredential;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DeactivateUser DeactivateUser(int userid, string ConnectionString)
        {
            DeactivateUser deactivateuser = new DeactivateUser();
            //bool response = false;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdDeactivateUser", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userid", userid);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        deactivateuser.flag = Convert.ToInt32(reader["FLAG"]);
                        deactivateuser.status = Convert.ToString(reader["STATUSMESSAGE"]);
                        deactivateuser.username = Convert.ToString(reader["USERNAME"]);

                    }
                    con.Close();
                }
                return deactivateuser;
            }
            catch (Exception ex) { throw ex; }

        }

        public bool UserLockStatusCheck(int? userId, int? patientId, string ConnectionString)
        {
            bool ret = true;
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UserLockStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    if (userId!=null)
                    {
                        command.Parameters.Add("@userid", SqlDbType.Int).Value = userId;
                    }
                    else if (patientId!=null)
                    {
                        command.Parameters.Add("@patientid", SqlDbType.Int).Value = patientId;
                    }
                    else
                    {
                        return false;
                    }
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
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

        public String[] GetLanguages (String ConnectionString)
        {
            
            try
            {
                string[] Languages = new string[2];
                int i = 0;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetLanguages", con);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        Languages[i] = Convert.ToString(reader["Language"]);
                        i++;
                    }
                   
                    con.Close();
                }
                return Languages;
            }
            catch (Exception ex) { throw ex; }

        }

        public string IsRoomExists(string UserName, string PatientId,string ConnectionString)
        {
            string room = null;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetVideoRoom", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    command.Parameters.AddWithValue("Patient", PatientId);
                    con.Open();
                    object response = command.ExecuteScalar();
                    if (response != DBNull.Value)
                    {
                        room = (string)response;

                    }
                    else
                    {
                        room = null;
                    }
                    con.Close();
                }
            }catch(Exception ex) { throw ex; }
            return room;
        }


        public string GetCommUserName(string UserName, string ConnectionString)
        {
            string commUserName = null;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetcommUserName", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    con.Open();
                    object response = command.ExecuteScalar();
                    if (response != DBNull.Value)
                    {
                        commUserName = (string)response;

                    }
                    else
                    {
                        commUserName = null;
                    }
                    con.Close();
                }
            }
            catch (Exception ex) { throw ex; }
            return commUserName;
        }

        public commUserNamesforVideoCall GetCommUserNamesforVideo(string CareTeam,string Patient, string ConnectionString)
        {
            commUserNamesforVideoCall response = new commUserNamesforVideoCall();
            
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetcommUserNamesofCTMandPatient", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", CareTeam);
                    command.Parameters.AddWithValue("Patient", Patient);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        
                        response.CommUserNameCareTeam = Convert.ToString(reader["CareTeamMember"]);
                        response.CommUserNamePatient = Convert.ToString(reader["Patient"]);

                    }
                    con.Close();
                }
            }
            catch (Exception ex) { throw ex; }
            return response;
        }





        public void  UpdateVideoRoom(string UserName, string PatientId,string RoomName, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdateVideoRoom", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName); 
                    command.Parameters.AddWithValue("Patient", PatientId);
                    command.Parameters.AddWithValue("RoomName", RoomName);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public void UpdateVideoRoomToken(string UserName, string Token, string roomname, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdVideoRoomToken", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    command.Parameters.AddWithValue("Token", Token);
                    command.Parameters.AddWithValue("RoomName", roomname);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }catch(Exception ex) { throw ex; }
            
        }

        public ChatDetails GetChatDetails(string UserName, string ConnectionString) 
        {
            ChatDetails chatdetails = new ChatDetails();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_Getchatdetails", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CreatedBy", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {  
                        chatdetails.token = (!DBNull.Value.Equals(reader["chatToken"])) ? Convert.ToString(reader["chatToken"]) : null;
                    }
                    if (chatdetails.token != null)
                    {
                        chatdetails.istoken = true;
                    }
                    con.Close();
                }
                return chatdetails;
            }
            catch (Exception ex) { throw ex; }  
        }
        public void UpdateChatDetails(string UserName,ChatDetails chatdetails, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdChatDetails", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    command.Parameters.AddWithValue("Token", chatdetails.token);
                    command.Parameters.AddWithValue("MakeIsactiveZero", chatdetails.makeisactivezero);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public bool UpdateChatResource(ChatResourceDetails chatresource, string ConnectionString)
        {
            try
            {
                bool ret = false;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdConversationDetails", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", chatresource.CreatedBy);
                    command.Parameters.AddWithValue("ToUser", chatresource.ToUser);
                    command.Parameters.AddWithValue("ConversationSid", chatresource.ConversationSid);
                    command.Parameters.AddWithValue("ChatToken", chatresource.ChatToken);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    con.Close();
                    if (id != 0)
                    {
                        ret = true;
                    }
                    return ret;
                }
            }
            catch (Exception ex) { throw ex; }
        }



        public string GetChatResource(string UserName,string ToUser, string ConnectionString)
        {
            try 
            {
                string conversationsid = null;
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand command = new SqlCommand("usp_GetConversationDetails", con);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", UserName);
                        command.Parameters.AddWithValue("ToUser", ToUser);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            conversationsid = (!DBNull.Value.Equals(reader["ConversationSid"])) ? Convert.ToString(reader["ConversationSid"]) : null;
                        }
                        con.Close();
                    }

                }
                return conversationsid;
            }
            catch (Exception ex) { throw ex; }
        }


        public List<string> GetAllConversationsAsync(string UserName, string ToUser, string AccountSIDValue, string AuthTokenValue, string ConnectionString)
        {
            try
            {
                List<string> conversations = new List<string>();
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand command = new SqlCommand("usp_GetAllConversations", con);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", UserName);
                        command.Parameters.AddWithValue("ToUser", ToUser);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        
                        while (reader.Read())
                        {
                            string conversationSid = (!DBNull.Value.Equals(reader["ConversationSid"])) ? Convert.ToString(reader["ConversationSid"]) : null;
                            if (conversationSid != null)
                            {
                                conversations.Add(conversationSid);
                                //TwilioClient.Init(AccountSIDValue, AuthTokenValue);
                                //// Retrieve messages from the conversation
                                //int limit = 30; // Number of messages to retrieve
                                //int skip = 0;   // Number of messages to skip (pagination)
                                //ChatDetails chatdetails = new ChatDetails();
                                //chatdetails.istoken = false;
                                //chatdetails = RpmDalFacade.GetChatDetails(ToUser);
    
                                //var conversations1 = ConversationResource.Read(limit: 10);

                                //var client = TwilioClient.GetRestClient();
                                //TwilioClient.SetRegion("us1");
                                //Console.WriteLine($"Authenticated as: {client.AccountSid}");
                                //var messages = MessageResource.ReadAsync(
                                //    pathConversationSid: "ConversationSid",
                                //    limit: 20);

                                //foreach (var record in messages)
                                //{
                                //    Console.WriteLine(record.AccountSid);
                                //}


                            }
                        }

                    }

                }
                return conversations;
            }
            catch (Exception ex) { throw ex; }
        }


        public  List<string> GetAllLoginSessions(string ConnectionString)
        {
            try
            {
                List<string> Tokens = new List<string>();

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetAllLoginSessions", con);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string jwtToken = Convert.ToString(reader["JwtToken"]);
                        Tokens.Add(jwtToken);
                    }

                    con.Close();
                }
                return Tokens;
            }
            catch (Exception ex) { throw ex; }
        }


        public  void UpdInvalidSessionZero(string jwtToken, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdInvalidSessionzero", con);
                    command.Parameters.AddWithValue("@JwtToken", jwtToken);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteReader();
                    con.Close();
                }

            }
            catch (Exception ex) { throw ex; }

        }
    }
}
