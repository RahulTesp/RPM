using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace RPM.Comm.Api
{
    public class TwilioCommon
    { 
    }
    public class SendSms
    {
        public string PhoneNo { get; set; }
        public string Message { get; set; }
    }
    public class AccessToken
    {
        public string identity { get; set; }
        public string token { get; set; }
    }
    public class ChatTwilio
    {
        public string chat_name { get; set; }
    }
    public class ParicipantTwilio
    {
        public string twilio_chatid { get; set; }
        public string chat_participant { get; set; }
    }
    public class MessageTwilio
    {
        public string twilio_chatid { get; set; }
        public string chat_participant { get; set; }
        public string chat_message { get; set; }
    }
    public class Chat_Sid
    {
        public string chat_sid { get; set; }
    }

    public class SendSmsToPatient
    {
        public string PatientUserName { get; set; }
        public string fromPhoneNo { get; set; }
        public string toPhoneNo { get; set; }
        public string Message { get; set; }
    }
}
