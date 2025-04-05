using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPMWeb.Common
{
    public enum SystemNotificationsMsgId
    {
        PatientEnrollmentPatient=0,
        PatientEnrollmentPhysician,
        PatientEnrollmentCareTeamAdmin,
        PatientDischargedPatient,
        PatientDischargedPhysician,
        PatientDischargedCareTeamAdmin,
        PatientReadyToDischargePhysician,
        PatientReadyToDischargeCareTeamAdmin,
        PatientProgramEndReminderCareTeam,
        PatientProgramEndButNotReadyToDischargeCareTeamAdmin,
        PatientProgramEndButNotReadyToDischargeCareTeam,
        PatientOnHoldPatient,
        PatientOnHoldPhysician,
        PatientOnHoldCareTeamAdmin,
        LoginOTPPatient,
        PasswordChange,
        CareTeamUpdatesPatient,
        CareTeamUpdatesPhysician,
        CareTeamUpdatesCareTeamAdmin,
        CareTeamUpdatesCareTeam,
        SchedulePatient,
        SchedulePhysician,
        ScheduleCareTeamAdmin,
        ScheduleCareTeam,
        ScheduleProvider,
        VitalScheduleUpdatePatient,
        VitalScheduleUpdatePhysician,
        VitalScheduleUpdateCareTeam,
        CriticalVitalAlertPhysician,
        VitalSLANotMetPhysician,
        VitalMissedPatient,
        MedicineUpdatedPatient,
        MedicineUpdatedPhysician,
        MedicineUpdatedCareTeam,
        VitalDeleteCareTeamAdmin,
        ChatNotificationCareTeam
    }
    public static class SystemNotificationMgr
    {
        public static void NotifyUser(string Msg, int UserId, string loggedUser,
                                     string connString,bool bSendSMS=false)
        {
            try
            {
                GetUserProfiles profile = new User().GetUserProfiles(UserId, loggedUser, connString);
                if(profile != null)
                {
                    AlertNotification alertNotification = new AlertNotification(NotificationType.NotificationRead.ToString(),
                                                                                 profile.UserName);
                    MsgQueueWrapper.PushToQueue(alertNotification, connString);
                    if(bSendSMS)
                    {
                        SendSms1 sendSms1 = new SendSms1(profile.MobileNo, Msg);
                        MsgQueueWrapper.SendMessage(sendSms1, connString);
                        
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public static string GetSystemNotificationMsg(SystemNotificationsMsgId id)
        {
            string msg = string.Empty;
            switch(id)
            {
                case SystemNotificationsMsgId.PatientEnrollmentPatient:
                    msg = @"You are succesfully enrolled into RPM program for {0} monitoring.
                            You userid to login to RPM application is {1}.
                            Please use the link to download our app.Webportal link: {2}.";
                    break;
                case SystemNotificationsMsgId.PatientEnrollmentPhysician:
                    msg = @"{0} has enrolled into RPM program for {1} monitoring.";
                    break;
                case SystemNotificationsMsgId.PatientEnrollmentCareTeamAdmin:
                    msg = @"{0} has enrolled into RPM program for {1} monitoring.";
                    break;
                case SystemNotificationsMsgId.PatientDischargedPatient:
                    msg = @"You are succesfully completed your  RPM program for {0} monitoring.";
                    break;
                case SystemNotificationsMsgId.PatientDischargedPhysician:
                    msg = @"{0} has completed RPM program for {1} monitoring.";
                    break;
                case SystemNotificationsMsgId.PatientDischargedCareTeamAdmin:
                    msg = @"{0} has completed RPM program for {1} monitoring.";
                    break;
                case SystemNotificationsMsgId.PatientReadyToDischargePhysician:
                    msg = @"{0}'s program duration is complete and Ready for Discharge, Please review.";
                    break;
                case SystemNotificationsMsgId.PatientReadyToDischargeCareTeamAdmin:
                    msg = @"{0}'s program duration is complete and Ready for Discharge, Please review.";
                    break;
                case SystemNotificationsMsgId.PatientProgramEndReminderCareTeam:
                    msg = @"Program is ending for patient {0} by {1},Please review";
                    break;
                case SystemNotificationsMsgId.PatientProgramEndButNotReadyToDischargeCareTeamAdmin:
                    msg = @"{0}'s program duration is ended, but Patient is not Discharged, Please check the Patient Status.";
                    break;
                case SystemNotificationsMsgId.PatientProgramEndButNotReadyToDischargeCareTeam:
                    msg = @"{0}'s program duration is ended, but Patient is not Discharged, Please check the Patient Status.";
                    break;
                case SystemNotificationsMsgId.PatientOnHoldPatient:
                    msg = @"Dear Patient, Your RPM service  is put  Onhold, Please call us to conitnue the service";
                    break;
                case SystemNotificationsMsgId.PatientOnHoldPhysician:
                    msg = @"{0}'s Status is Onhold";
                    break;
                case SystemNotificationsMsgId.PatientOnHoldCareTeamAdmin:
                    msg = @"{0}'s Status is Onhold";
                    break;
                case SystemNotificationsMsgId.LoginOTPPatient:
                    msg = @"Your one time password to login to RPM application is {0}. Do not share OTP with anyone";
                    break;
                case SystemNotificationsMsgId.PasswordChange:
                    msg = @"Password change is successfully completed. If this is not done by you, please contact the customer care number.";
                    break;
                case SystemNotificationsMsgId.CareTeamUpdatesPatient:
                    msg = @"Dear Patient, Your new Care Team Member is {0}.";
                    break;
                case SystemNotificationsMsgId.CareTeamUpdatesPhysician:
                    msg = @"Care Team Member for {0} is changed to {1}.";
                    break;
                case SystemNotificationsMsgId.CareTeamUpdatesCareTeamAdmin:
                    msg = @"Care Team Member for {0} is changed to {1}.";
                    break;
                case SystemNotificationsMsgId.CareTeamUpdatesCareTeam:
                    msg = @"Patient {0} is added to your patient list.";
                    break;
                case SystemNotificationsMsgId.SchedulePatient:
                    msg = @"You have an appointment scheduled on {0} with your {1}.";
                    break;
                case SystemNotificationsMsgId.SchedulePhysician:
                    msg = @"You have an appointment scheduled on {0} with {1}.";
                    break;
                case SystemNotificationsMsgId.ScheduleCareTeamAdmin:
                    msg = @"You have an appointment scheduled on {0} with {1}.";
                    break;
                case SystemNotificationsMsgId.ScheduleCareTeam:
                    msg = @"You have an appointment scheduled on {0} with {1}.";
                    break;
                case SystemNotificationsMsgId.ScheduleProvider:
                    msg = @"You have an appointment scheduled on {0} with {1}.";
                    break;
                case SystemNotificationsMsgId.VitalScheduleUpdatePatient:
                    msg = @"Your Vital reading schedule is updated. Kindly follow the new schedule.";
                    break;
                case SystemNotificationsMsgId.VitalScheduleUpdatePhysician:
                    msg = @"Vital schedule for {0} is updated, please review.";
                    break;
                case SystemNotificationsMsgId.VitalScheduleUpdateCareTeam:
                    msg = @"Vital schedule for {0} is updated, please review.";
                    break;
                case SystemNotificationsMsgId.CriticalVitalAlertPhysician:
                    msg = @"{0} for {1} is in critical range with value {2} on {3}.";
                    break;
                case SystemNotificationsMsgId.VitalSLANotMetPhysician:
                    break;
                case SystemNotificationsMsgId.VitalMissedPatient:
                    msg = @"You have not taken readings yesterday, Please take the vital readings without fail.";
                    break;
                case SystemNotificationsMsgId.MedicineUpdatedPatient:
                    msg = @"Dear Patient, Your medicine chart is updated, please check.";
                    break;
                case SystemNotificationsMsgId.MedicineUpdatedPhysician:
                    msg = @"{0}'s medicine chart is updated, please review.";
                    break;
                case SystemNotificationsMsgId.MedicineUpdatedCareTeam:
                    msg = @"{0}'s medicine chart is updated, please review.";
                    break;
                case SystemNotificationsMsgId.VitalDeleteCareTeamAdmin:
                    msg = @"{0}'s vital reading is deleted ,please review";
                    break;
                case SystemNotificationsMsgId.ChatNotificationCareTeam:
                    msg = @"{0} has initiated a chat . Click here to start the chat.";
                    break;
            }
            return msg;
        }

    }
}
