using Microsoft.Extensions.Configuration;
using RPMWeb.Common;
using RPMWeb.Dal;
using RPMWeb.Data.Common;

namespace RPMCriticalAlertJob
{
    public class UserNotificationMgr
    {
        static string CONN_STRING = string.Empty;
        public void VerifyAndSendUserNotifications()
        {
            // Set up configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables() // Allows overriding via Azure App Settings
                .Build();

            // Access a specific config value
            string connStr = config["RPM:ConnectionString"];
            Console.WriteLine($"RPM Connection String: {connStr}");

            // Optional: bind strongly-typed object
            var rpmSettings = config.GetSection("RPM").Get<RpmSettings>();
            Console.WriteLine($"RPM.ConnectionString (typed): {rpmSettings?.ConnectionString}");
            CONN_STRING = rpmSettings?.ConnectionString;
            Task task = new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        //1. Check the patient status
                        SendPatientReminders();
                        //2. Send the notifications
                        SendPatientNotifications();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                }

            });
            task.Start();
        }
        private void SendPatientNotifications()
        {
            try
            {
                Notification inst = new Notification();
                List<SystemNotification> systemNotifications = inst.GetSystemNotifications(false, CONN_STRING);
                if (systemNotifications != null && systemNotifications.Count > 0)
                {
                    foreach (SystemNotification systemNotification in systemNotifications)
                    {
                        GetPatientInfo info = null;
                        if (systemNotification.UserRole == "Patient")
                        {
                            Patient patient = new Patient();
                            info = patient.GetPatientInfo(systemNotification.UserName, CONN_STRING).First();

                        }
                        foreach (NotificationData data in systemNotification.NotificationData)
                        {
                            if (((data.SubType == "PatientEnrolled" && systemNotification.UserRole == "Patient") ||
                               (data.SubType == "PatientDischarged" && systemNotification.UserRole == "Patient")) &&
                               info != null)
                            {
                                SendSms1 obj = new SendSms1();
                                obj.PhoneNo = info.EmergencyContactNumber1;
                                obj.Message = data.Description;
                                MsgQueueWrapper.SendMessage(obj, CONN_STRING);
                            }
                            if (data.NotificationAuditId == 0)
                            {
                                NotificationAuditData auditData = new NotificationAuditData();
                                auditData.NotificationId = data.NotificationId;
                                auditData.IsRead = false;
                                auditData.IsNotify = true;
                                auditData.AuditCreatedBy = "System";
                                inst.AddSystemNotificationAudit(auditData, CONN_STRING);
                            }
                            else
                            {
                                inst.UpdateSystemNotificationAuditNotifyStatus(data.NotificationAuditId, true, CONN_STRING);
                            }
                        }
                        AlertNotification an = new AlertNotification();
                        an.EventType = NotificationType.NotificationRead.ToString();
                        an.User = systemNotification.UserName;
                        MsgQueueWrapper.PushToQueue(an, CONN_STRING);
                    }
                }

            }
            catch
            {
                throw;
            }
        }
        private void SendPatientReminders()
        {
            try
            {
                PatientProgram inst = new PatientProgram();
                List<PatientProgramDetails> lstDetails = inst.GetAllPatientProgramDetails(CONN_STRING);
                foreach(PatientProgramDetails details in lstDetails)
                {
                    Notification notification = new Notification();
                    if (DateTime.Now.Date> details.ProgramEndDate.Date && 
                        (!details.ProgramStatus.Equals("Discharged") ||
                        !details.ProgramStatus.Equals("ReadyToDischarge")))
                    {
                        SystemNotification_ins sn = new SystemNotification_ins();
                        sn.RecId = details.Id;
                        sn.UserId = details.CareTeamMemberId;
                        sn.NotificationTypeId = 34;
                        sn.Desc = string.Format(@"{0} {1} ({2})'s program duration is ended,
                                                  but Patient is not Discharged. Please check the Patient Status.",
                                                  details.PatientFirstName, details.PatientLastName,
                                                  details.PatientUserName,
                                                  details.ProgramEndDate.Date.ToShortDateString());
                        sn.CreatedBy = "System";
                        notification.AddSystemNotification(sn, CONN_STRING);
                    }
                    else if(DateTime.Now.AddDays(7).Date == details.ProgramEndDate.Date)
                    {
                        SystemNotification_ins sn = new SystemNotification_ins();
                        sn.RecId = details.Id;
                        sn.UserId = details.CareTeamMemberId;
                        sn.NotificationTypeId = 33;
                        sn.Desc = string.Format(@"Program is ending for patient{0} {1} ({2}) by {3}, Please review.",
                                                 details.PatientFirstName, details.PatientLastName,
                                                 details.PatientUserName);
                        sn.CreatedBy = "System";
                        notification.AddSystemNotification(sn, CONN_STRING);
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
