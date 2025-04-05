using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPMWeb.Data.Common
{
    public enum NotificationType
    {
        PriorityAlert = 0,
        NotificationRead,
        AlertRead,
        SendSms
    }

    class Notification
    {

    }
    public class AlertNotification
    {
        public AlertNotification() { }
        public AlertNotification(string evttype, string usrname)
        {
            EventType = evttype; User= usrname;
        }
        public string EventType { get; set; }
        public string User { get; set; }
        //public object Data { get; set; }
    }
    public class SendSms1
    {
        public SendSms1() { }
        public SendSms1(string phno, string msg)
        {
            PhoneNo = phno; Message= msg; 
        }
        public string PhoneNo { get; set; }
        public string Message { get; set; }
    }
}
