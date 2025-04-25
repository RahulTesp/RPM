using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using iText.Kernel.Pdf.Action;

namespace RPMWeb.Dal
{
    public class Notification
    {
        public int AddPatientProgramMedication(PatientNotification_ins data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientNotifications", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@Description", data.Description);
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
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
            catch (Exception)
            {
                throw;
            }

        }
        public int UpdatePatientProgramMedication(PatientNotificationReadStatus data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientNotificationsStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", data.Id);
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@ReadStatus", data.ReadStatus);
                    command.Parameters.AddWithValue("@ModifiedBy", data.ModifiedBy);
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
        public SystemNotificationByUser GetSystemNotificationsByUser(string UserName, DateTime? StartDate,
                                                     int Count, string user, string ConnectionString)
        {
            SystemNotificationByUser ret = new SystemNotificationByUser();
            PatientDetials pdt = new PatientDetials();
            pdt.PatientId = 0;
            pdt.PatientId = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetSystenNotificationsByDate", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                    command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = StartDate;
                    command.Parameters.Add("@Count", SqlDbType.Int).Value = Count;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = user;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string username = (!DBNull.Value.Equals(reader["UserName"])) ? reader["UserName"].ToString() : string.Empty;
                        string CreatedBy = (!DBNull.Value.Equals(reader["CreatedBy"])) ? reader["CreatedBy"].ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(CreatedBy))
                        {
                            pdt = GetPatientAndProgramByUserName(CreatedBy, ConnectionString);

                        }
                        string userrole = (!DBNull.Value.Equals(reader["RoleName"])) ? reader["RoleName"].ToString() : string.Empty;
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userrole)) continue;
                        NotificationData info = new NotificationData();
                        info.NotificationId = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.NotificationAuditId = (!DBNull.Value.Equals(reader["AuditId"])) ? Convert.ToInt32(reader["AuditId"]) : 0;
                        info.Description = (!DBNull.Value.Equals(reader["Description"])) ? reader["Description"].ToString() : string.Empty;
                        info.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                        info.Type = (!DBNull.Value.Equals(reader["Name"])) ? reader["Type"].ToString() : string.Empty;
                        info.SubType = (!DBNull.Value.Equals(reader["Type"])) ? reader["Name"].ToString() : string.Empty;
                        info.IsRead = (!DBNull.Value.Equals(reader["IsRead"])) ? Convert.ToBoolean(reader["IsRead"]) : false;
                        info.IsNotify = (!DBNull.Value.Equals(reader["IsNotify"])) ? Convert.ToBoolean(reader["IsNotify"]) : true;
                        info.PatientId = pdt.PatientId;
                        info.ProgramId = pdt.ProgramId;

                        NotificationDataByDate nd = ret.Data.Find(x => x.NotificationDate.Date.Equals(info.CreatedOn.Date));
                        if (nd != null)
                        {
                            nd.NotificationList.Add(info);
                        }
                        else
                        {
                            NotificationDataByDate ndtemp = new NotificationDataByDate();
                            ndtemp.NotificationDate = info.CreatedOn.Date;
                            ndtemp.NotificationList.Add(info);
                            ret.Data.Add(ndtemp);
                        }
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        ret.TotalNotifications = (!DBNull.Value.Equals(reader["TotalCount"])) ? Convert.ToInt32(reader["TotalCount"]) : 0;
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        ret.TotalUnRead = (!DBNull.Value.Equals(reader["TotalUnread"])) ? Convert.ToInt32(reader["TotalUnread"]) : 0;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public PatientDetials GetPatientAndProgramByUserName(string username, string connectionString)
        {
            PatientDetials pdt = new PatientDetials();
            pdt.PatientId = 0;
            pdt.PatientId = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("GetPatientAndProgramByUserName", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@InputUserName", username);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int patientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0;
                            int patientProgramId = reader["PatientProgramId"] != DBNull.Value ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                            pdt.PatientId = patientId;
                            pdt.PatientId = patientProgramId;
                            return pdt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving patient and program by username", ex);
            }

            return pdt;
        }

        public SystemNotificationCount GetSystemNotificationCount(string UserName, string ConnectionString)
        {
            SystemNotificationCount ret = new SystemNotificationCount();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetSystenNotificationCount", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = UserName;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.TotalNotifications = (!DBNull.Value.Equals(reader["TotalCount"])) ? Convert.ToInt32(reader["TotalCount"]) : 0;
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        ret.TotalUnRead = (!DBNull.Value.Equals(reader["TotalUnread"])) ? Convert.ToInt32(reader["TotalUnread"]) : 0;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public List<SystemNotification> GetSystemNotifications(bool bIsNotify, string ConnectionString)
        {
            List<SystemNotification> ret = new List<SystemNotification>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetSystemNotifications", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@IsNotify", SqlDbType.Int).Value = bIsNotify;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "System";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string username = (!DBNull.Value.Equals(reader["UserName"])) ? reader["UserName"].ToString() : string.Empty;
                        string userrole = (!DBNull.Value.Equals(reader["RoleName"])) ? reader["RoleName"].ToString() : string.Empty;
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userrole)) continue;
                        NotificationData info = new NotificationData();
                        info.NotificationId = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.NotificationAuditId = (!DBNull.Value.Equals(reader["AuditId"])) ? Convert.ToInt32(reader["AuditId"]) : 0;
                        info.Description = (!DBNull.Value.Equals(reader["Description"])) ? reader["Description"].ToString() : string.Empty;
                        info.CreatedOn = (!DBNull.Value.Equals(reader["CreatedOn"])) ? Convert.ToDateTime(reader["CreatedOn"]) : DateTime.MinValue;
                        info.Type = (!DBNull.Value.Equals(reader["Name"])) ? reader["Type"].ToString() : string.Empty;
                        info.SubType = (!DBNull.Value.Equals(reader["Type"])) ? reader["Name"].ToString() : string.Empty;
                        info.IsRead = (!DBNull.Value.Equals(reader["IsRead"])) ? Convert.ToBoolean(reader["IsRead"]) : false;
                        info.IsNotify = (!DBNull.Value.Equals(reader["IsNotify"])) ? Convert.ToBoolean(reader["IsNotify"]) : true;
                        SystemNotification si = ret.Find(x => x.UserName.Equals(username));
                        if (si == null)
                        {
                            si = new SystemNotification();
                            si.UserName = username;
                            si.UserRole = userrole;
                            ret.Add(si);
                        }
                        si.NotificationData.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public int AddSystemNotification(SystemNotification_ins data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsSystemNotification", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RecId", data.RecId);
                    command.Parameters.AddWithValue("@UserId", data.UserId);
                    command.Parameters.AddWithValue("@NotificationTypeId", data.NotificationTypeId);
                    command.Parameters.AddWithValue("@Description", data.Desc);
                    command.Parameters.AddWithValue("@Createdby", data.CreatedBy);
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
            catch (Exception)
            {
                throw;
            }
        }

        public int AddSystemNotificationAudit(NotificationAuditData data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsSystemNotificationAudit", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SysNotificationId", data.NotificationId);
                    command.Parameters.AddWithValue("@IsRead", data.IsRead);
                    command.Parameters.AddWithValue("@IsNotify", data.IsNotify);
                    command.Parameters.AddWithValue("@Createdby", data.AuditCreatedBy);
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
            catch (Exception)
            {
                throw;
            }
        }
        public int UpdateSystemNotificationAuditReadStatus(int notificationId, bool status, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdSystemNotficationAuditReadStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@NotificationId", notificationId);
                    command.Parameters.AddWithValue("@IsRead", status);
                    command.Parameters.AddWithValue("@ModifiedBy", "System");
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    return notificationId;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public int UpdateSystemNotificationAuditNotifyStatus(int notificationId, bool status, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdSystemNotficationAuditNotifyStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@NotificationId", notificationId);
                    command.Parameters.AddWithValue("@IsNotify", status);
                    command.Parameters.AddWithValue("@ModifiedBy", "System");
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


        public bool DeleteSystemNotificationsByUser(int notificationId, string UserName, string ConnectionString)
        {
            bool response = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_DelSystemNotifications", con);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("NotificationId", notificationId);
                command.Parameters.AddWithValue("CreatedBy", UserName);
                con.Open();
                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
                int r = (int)returnParameter.Value;
                if (!r.Equals(0))
                { response = true; }
                con.Close();
                return response;
            }

        }
        public bool DeleteSystemNotificationsReadUnRead(int notificationId, string UserName, string ConnectionString)
        {
            bool response = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_DeleteNotificationsReadUnread", con);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("NotificationId", notificationId);
                command.Parameters.AddWithValue("CreatedBy", UserName);
                con.Open();
                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
                int r = (int)returnParameter.Value;
                if (!r.Equals(0))
                { response = true; }
                con.Close();
                return response;
            }

        }

        public void GetFirebaseNotificationByUser(string UserName, string ReceiverId, firebasenotificationmessage notify, string category, string ConnectionString)
        {
            List<string> FirebaseTokens = new List<string>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_GetFireBaseToken", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("CreatedBy", UserName);
                command.Parameters.AddWithValue("ReceiverId", ReceiverId);
                command.Parameters.AddWithValue("BearerId", 0);

                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fbk = (!DBNull.Value.Equals(reader["FbToken"])) ? reader["FbToken"].ToString() : string.Empty;
                        FirebaseTokens.Add(fbk);
                    }
                }
                con.Close();

            }

            SentFirebaseNotification(notify, category, FirebaseTokens);

        }

        public void GetFirebaseNotificationCallRejection(string UserName, string ReceiverId, firebasenotificationmessage notify, string category, string ConnectionString, int bearerid)
        {
            List<string> FirebaseTokens = new List<string>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_GetFireBaseToken", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("CreatedBy", UserName);
                command.Parameters.AddWithValue("ReceiverId", ReceiverId);
                command.Parameters.AddWithValue("BearerId", bearerid);

                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fbk = (!DBNull.Value.Equals(reader["FbToken"])) ? reader["FbToken"].ToString() : string.Empty;
                        FirebaseTokens.Add(fbk);
                    }
                }
                con.Close();

            }

            SentFirebaseNotification(notify, category, FirebaseTokens);

        }

        public void SentFirebaseNotification(firebasenotificationmessage notify, string category, List<string> FirebaseTokens)
        {
            foreach (string fbk in FirebaseTokens)
            {
                try
                {
                    string fileName = "rpmadmin-key.json";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        var fire = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(filePath)
                        });
                        var fireauth = FirebaseAuth.GetAuth(fire);
                        var registrationToken = fbk;
                        var message = new Message()
                        {
                            Token = registrationToken,

                            Data = new Dictionary<string, string>()
                            {
                                { "Title" , notify.title},
                                {  "Body"  , notify.body },

                            },
                            //Notification = new FirebaseAdmin.Messaging.Notification()
                            //{
                            //    Title = notify.title,
                            //    Body = notify.body
                            //}

                        };
                        string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                        Console.WriteLine("Successfully sent message: " + response);
                    }
                    else
                    {
                        var registrationToken = fbk;
                        var message = new Message()
                        {
                            Token = registrationToken,

                            Data = new Dictionary<string, string>()
                            {
                                { "Title" , notify.title},
                                {  "Body"  , notify.body }
                            },

                            //Notification = new FirebaseAdmin.Messaging.Notification()
                            //{
                            //    Title = notify.title,
                            //    Body = notify.body
                            //}

                        };
                        Console.WriteLine(message.Notification);
                        string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                        Console.WriteLine("Successfully sent message: " + response);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Sending Notification to", fbk);
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public bool InsertFirebaseToken(string UserName, string Bearer, string Token, string ConnectionString)
        {
            bool response = false;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsFirebaseToken", con);

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    command.Parameters.AddWithValue("Bearer", Bearer);
                    command.Parameters.AddWithValue("FirebaseToken", Token);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    int r = (int)returnParameter.Value;
                    if (!r.Equals(0))
                    { response = true; }
                    con.Close();
                }
            }
            catch (Exception ex) { throw; }
            return response;
        }

        public bool IsPatientOnline(string PatientId, string UserName, string ConnectionString)
        {
            bool isOnline = false;
            try
            {

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_IsPatientOnline", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("CreatedBy", UserName);
                    command.Parameters.AddWithValue("PatientUserName ", PatientId);
                    con.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    int r = (int)returnParameter.Value;
                    if (r.Equals(1))
                    { isOnline = true; }
                    con.Close();
                }
            }
            catch (Exception ex) { throw; }
            return isOnline;
        }
        public void StoreGenericFireBaseNotifications(string username, string notification, string FromUser, string CONN_STRING)
        {
            try
            {
                int UserId = GetUserId(username, CONN_STRING);
                Notification fcmnotification = new Notification();
                SystemNotification_ins sn = new SystemNotification_ins();
                sn.RecId = 0;
                sn.UserId = UserId;
                sn.NotificationTypeId = 35;
                sn.Desc = notification;
                sn.CreatedBy = FromUser;
                int notificationid = fcmnotification.AddSystemNotification(sn, CONN_STRING);
                NotificationAuditData auditData = new NotificationAuditData();
                auditData.NotificationId = notificationid;
                auditData.IsRead = false;
                auditData.IsNotify = true;
                auditData.AuditCreatedBy = FromUser;
                fcmnotification.AddSystemNotificationAudit(auditData, CONN_STRING);

            }
            catch
            {
                throw;
            }
        }
        public void SendGenericFireBaseNotifications(firebasenotificationmessage notify, string category, List<string> FirebaseTokens)
        {
            try
            {
                foreach (string fbk in FirebaseTokens)
                {
                    try
                    {
                        string fileName = "rpmadmin-key.json";
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                        if (FirebaseApp.DefaultInstance == null)
                        {
                            var fire = FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromFile(filePath)
                            });
                            var fireauth = FirebaseAuth.GetAuth(fire);
                            var registrationToken = fbk;
                            var message = new Message()
                            {
                                Token = registrationToken,

                                Data = new Dictionary<string, string>()
                                {
                                    { "Title" , notify.title},
                                    {  "Body"  , notify.body },
                                },
                                Notification = new FirebaseAdmin.Messaging.Notification()
                                {
                                    Title = notify.title,
                                    Body = notify.body
                                }

                            };
                            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                            Console.WriteLine("Successfully sent message: " + response);
                        }
                        else
                        {
                            var registrationToken = fbk;
                            var message = new Message()
                            {
                                Token = registrationToken,

                                Data = new Dictionary<string, string>()
                                {
                                    { "Title" , notify.title},
                                    {  "Body"  , notify.body }
                                },

                                Notification = new FirebaseAdmin.Messaging.Notification()
                                {
                                    Title = notify.title,
                                    Body = notify.body
                                }

                            };
                            Console.WriteLine(message.Notification);
                            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                            Console.WriteLine("Successfully sent message: " + response);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error Sending Notification to", fbk);
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public List<string> GetGenericFirebaseTokens(string ReceiverName, string ConnectionString)
        {
            List<string> FirebaseTokens = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetUserFireBaseToken", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("ReceiverId", ReceiverName);
                    con.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fbk = (!DBNull.Value.Equals(reader["FbToken"])) ? reader["FbToken"].ToString() : string.Empty;
                            FirebaseTokens.Add(fbk);
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return FirebaseTokens;
        }

        private int GetUserId(string username, string ConnectionString)
        {
            int userId = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetUserId", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("UserName", username);
                    SqlParameter userIdParam = new SqlParameter("@UserId", SqlDbType.Int);
                    userIdParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(userIdParam);
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                    userId = (int)userIdParam.Value;

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return userId;
        }
        public bool NotifyPatientStatusChange(string status, PatientDetails patient, string UserName, string ConnString)
        {
            try
            {
                string body = "Status Changed";
                string msg = string.Empty;
                switch (status.ToLower())
                {
                    case "active":
                        msg = patient.FirstName + " " + patient.LastName + " is now Active.";
                        body = "Status Changed to Active";
                        break;
                    case "readytodischarge":
                        msg = patient.FirstName + " " + patient.LastName + " is now Ready To Discharge.";
                        body = "Status Changed to Ready To Discharge";
                        break;
                    case "discharged":
                        msg = patient.FirstName + " " + patient.LastName + " is now Discharged.";
                        body = "Status Changed to Discharged";
                        break;
                    default:
                        break;
                }
                firebasenotificationmessage notify = new firebasenotificationmessage();
                notify.title = msg;
                notify.body = body;
                string category = "PatientStatus";
                CareTeamDeatis team = GetCareTeamPhysician(patient.PatientId, UserName, ConnString);
                //notify Care Team Member
                NotifyCareTeam(team.CareTeamUserId, ConnString, UserName, msg, category, notify);
                //notify Physician
                NotifyCareTeam(team.PhysicianId, ConnString, UserName, msg, category, notify);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }
        public bool ConversationNotification(PatientDetails patient, string UserName, string ConnString)
        {
            try
            {
                string body = "Message Received from";
                string msg = string.Empty;
                firebasenotificationmessage notify = new firebasenotificationmessage();
                notify.title = msg;
                notify.body = body;
                string category = "PatientChat";
                CareTeamDeatis team = GetCareTeamPhysician(patient.PatientId, UserName, ConnString);
                //notify Care Team Member
                NotifyCareTeam(team.CareTeamUserId, ConnString, UserName, msg, category, notify);
                //notify Physician
                NotifyCareTeam(team.PhysicianId, ConnString, UserName, msg, category, notify);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }
        private bool NotifyCareTeam(int CareTeamId, string ConnString, string UserName, string msg, string category, firebasenotificationmessage notify)
        {
            try
            {
                string Receiver = GetUserName(CareTeamId, ConnString);
                StoreGenericFireBaseNotifications(Receiver, msg, UserName, ConnString);
                List<string> FireBaseTokens = GetGenericFirebaseTokens(Receiver, ConnString);
                SendGenericFireBaseNotifications(notify, category, FireBaseTokens);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return true;
        }
        private CareTeamDeatis GetCareTeamPhysician(int PatientId, string UserName, string ConnString)
        {
            CareTeamDeatis team = new CareTeamDeatis();
            int physicianId = 0;
            int careTeamUserId = 0;

            try
            {
                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPhysicianAndCareTeam", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", PatientId);

                    SqlParameter physicianIdParam = new SqlParameter("@PhysicianId", SqlDbType.Int);
                    physicianIdParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(physicianIdParam);

                    SqlParameter careTeamUserIdParam = new SqlParameter("@CareTeamUserId", SqlDbType.Int);
                    careTeamUserIdParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(careTeamUserIdParam);

                    con.Open();
                    command.ExecuteNonQuery();

                    physicianId = (int)(physicianIdParam.Value != DBNull.Value ? physicianIdParam.Value : 0);
                    careTeamUserId = (int)(careTeamUserIdParam.Value != DBNull.Value ? careTeamUserIdParam.Value : 0);
                    team.PhysicianId = physicianId;
                    team.CareTeamUserId = careTeamUserId;
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("Error retrieving physician and care team information", ex);
            }
            return team;
        }


        private string GetUserName(int UserId, string ConnectionString)
        {
            string UserName = string.Empty;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetUserName", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("UserId", UserId);
                    SqlParameter userNameParam = new SqlParameter("@UserName", SqlDbType.NVarChar);
                    userNameParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(userNameParam);
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                    UserName = (string)userNameParam.Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return UserName;
        }
    }
    public class CareTeamDeatis
    {
        public int PhysicianId { get; set; }
        public int CareTeamUserId { get; set; }
    }
}
