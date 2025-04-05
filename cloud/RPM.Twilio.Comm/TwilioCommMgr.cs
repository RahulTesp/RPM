using Twilio;
using Twilio.Types;
using Twilio.Rest.Conversations.V1;
using Twilio.Rest.Conversations.V1.Conversation;
using MessageResource = Twilio.Rest.Conversations.V1.Conversation.MessageResource;
using Twilio.Jwt.AccessToken;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Newtonsoft.Json;

namespace RPM.Comm.Api
{
    public class TwilioCommMgr
    {
        public bool MessagingService(SendSms Info, string _accsid, string _authtoken, string _smssid)
        {
            try
            {
                //var accountSid = "AC8b632d400f06b01cc2ab60b75fd6dc09";
                //var authToken = "285101ea9d75a31300f3e5a4be59ccd7";
                TwilioClient.Init(_accsid, _authtoken);
                var messageOptions = new Twilio.Rest.Api.V2010.Account.CreateMessageOptions(new PhoneNumber(Info.PhoneNo));
                messageOptions.MessagingServiceSid = _smssid;
                messageOptions.Body = Info.Message;

                var message = Twilio.Rest.Api.V2010.Account.MessageResource.Create(messageOptions);
                return true;
            }
            catch
            {
                throw;
            }
        }
        public bool MessagingServiceTopatient(SendSmsToPatient Info, string _accsid, string _authtoken, string _smssid, string username)
        {
            try
            {
                //var accountSid = "AC8b632d400f06b01cc2ab60b75fd6dc09";
                //var authToken = "285101ea9d75a31300f3e5a4be59ccd7";
                TwilioClient.Init(_accsid, _authtoken);
                var messageOptions = new Twilio.Rest.Api.V2010.Account.CreateMessageOptions(new PhoneNumber(Info.toPhoneNo));
                messageOptions.MessagingServiceSid = _smssid;
                messageOptions.Body = Info.Message;

                var message = Twilio.Rest.Api.V2010.Account.MessageResource.Create(messageOptions);
                SaveSmsInfo saveinfo=new SaveSmsInfo();
                saveinfo.PatientUserName= Info.PatientUserName;
                saveinfo.fromNo = Info.fromPhoneNo;
                saveinfo.toNo= Info.toPhoneNo;
                saveinfo.Body = message.Body;
                saveinfo.Status = message.Status.ToString();
                saveinfo.SentDate = (DateTime)message.DateCreated;
                saveinfo.Direction = message.Direction.ToString();
                RpmDalFacade.UpdatePatientSmsDetails(username,saveinfo);
                

                return true;
            }
            catch
            {
                throw;
            }
        }
        public AccessToken GetCallToken(string _accsid, string _twimlappsid,string _twilioapikey, string _twiliosecretkey, string mynumber)
        {
            try
            {
                AccessToken accT = new AccessToken();
                //var twilioAccountSid = "AC8b632d400f06b01cc2ab60b75fd6dc09"; //account sid
                //var appSid = "AP6898735862db43b26b114f959a6b528d"; // TwiMLAppSID
                //var twilioApiKey = "SKaeccd1b49611d2d12f64024ba148d66a";//APi Key SID
               // var twilioApiSecret = "cgTpYDT04oR4fzF2lJBSU0ghXRrl5T8o";//APIkey Secrete
                var grant = new VoiceGrant();
                grant.OutgoingApplicationSid = _twimlappsid;
                grant.IncomingAllow = true;
                var grants = new HashSet<IGrant>
                {
                    { grant }
                };
                string role = "user"; //Default role
                var accessToken = new Token(
                    _accsid,//twilioAccountSid,
                    _twilioapikey,//twilioApiKey,
                    _twiliosecretkey,//twilioApiSecret,
                    role,
                    grants: grants);
                accT.identity = mynumber;// "+16069340321"; //MyNumber
                accT.token = accessToken.ToJwt();
                return accT;
            }
            catch
            {
                throw;
            }
        } //Get Twilio token method impl
        public string CallConnect(string _tonumber, string _mynumber)
        {
            try
            {
                //Uri callRecord = new Uri("https://fdbb-116-68-110-105.ngrok.io/Test/CallRecord");
                //// var toNumber = HttpContext.Current.Request.Params["To"];
                ////var toNumber = _tonumber; // "+919742357950";
                ////var PhoneNumber = _mynumber;// "+19105974206";
                //var response = new VoiceResponse();
                //var dial = new Dial(callerId: _mynumber, record: "true", recordingStatusCallback: callRecord);
                //dial.Number(_tonumber);
                //response.Append(dial);
                //var res = new TwiMLResult(response).Data;
                //return res;

                Uri callRecord = new Uri("https://fdbb-116-68-110-105.ngrok.io/Test/CallRecord");
                var response = new VoiceResponse();
                var dial = new Dial(callerId: _mynumber, record: "true", recordingStatusCallback: callRecord);
                dial.Number(_tonumber);
                response.Append(dial);
                var res = response.ToString(); // Fix: Convert the response to string
                return res;
            }
            catch
            {
                throw;
            }

            /*HttpResponseMessage resp = Request.CreateResponse();
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent(res, Encoding.UTF8, "application/xml");
            return resp;*/

        }
        
        public string CreateConversation(ChatTwilio obj)
        {
            string accountSid = "AC458e8fdb70b332b9ceb6e9502d70d1ca";
            string authToken = "e7b81ffc684883806d51215749931bb0";

            TwilioClient.Init(accountSid, authToken);
            var chatName = obj.chat_name;
            var conversation = ConversationResource.Create(
                friendlyName: chatName
            );
            // var resp = Request.CreateResponse(HttpStatusCode.OK, conversation.Sid);
            return conversation.Sid;
        }
        public string AddParticipant(ParicipantTwilio obj)
        {
            try
            {
                var chatid = obj.twilio_chatid;
                string accountSid = "AC458e8fdb70b332b9ceb6e9502d70d1ca";
                string authToken = "e7b81ffc684883806d51215749931bb0";
                var chatParticipant = obj.chat_participant;
                TwilioClient.Init(accountSid, authToken);

                var participant = ParticipantResource.Create(
                 identity: chatParticipant,
                 pathConversationSid: chatid);
                return participant.Sid;
            }
            catch
            {
                throw;
            }
        }
        public string CreateMsg(MessageTwilio obj)
        {
            try
            {
                string accountSid = "AC458e8fdb70b332b9ceb6e9502d70d1ca";
                string authToken = "e7b81ffc684883806d51215749931bb0";

                TwilioClient.Init(accountSid, authToken);


                var message = MessageResource.Create(
                   author: obj.chat_participant,
                   body: obj.chat_message,
                   pathConversationSid: obj.twilio_chatid
               );
                return message.Sid;
            }
            catch
            { 
                throw;
            }
  
          /*  Console.WriteLine(message.Sid);

            HttpResponseMessage resp = Request.CreateResponse();
            resp.StatusCode = HttpStatusCode.OK;
            var res = "1";
            resp.Content = new StringContent(message.Sid, Encoding.UTF8, "application/xml");
            var connString = "Endpoint=https://rpmpubsub.webpubsub.azure.com;AccessKey=S/hyzD4IqIf8yYkFTWJdeolnVVQiWuBcPQgI85rUSIc=;Version=1.0;";
            var hubname = "rpm_chat";
            var serviceClient = new WebPubSubServiceClient(connString, hubname);
            serviceClient.SendToAll(RequestContent.Create(new { Foo = "You have a notification...!" }), ContentType.ApplicationJson);
            return resp;*/
        }
        public string getconversations(Chat_Sid obj)
        {
            var pathSid_val = obj.chat_sid;
            string accountSid = "AC458e8fdb70b332b9ceb6e9502d70d1ca";
            string authToken = "e7b81ffc684883806d51215749931bb0";

            TwilioClient.Init(accountSid, authToken);


            var conversations = ConversationResource.Read(limit: 20);
            //var json = new JavaScriptSerializer().Serialize(conversations);
            string json = JsonConvert.SerializeObject(conversations);
            object[] array = new object[5];
            foreach (var record in conversations)
            {
                Console.WriteLine(record.Links);
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://conversations.twilio.com/v1/Conversations/CH4b318a213fb54f9c827aa435e8629989/");
                    //HTTP GET
                    var responseTask = client.GetAsync("Messages");
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                    }
                }
            }
            return String.Empty;

          /*  HttpResponseMessage resp = Request.CreateResponse();
            resp.StatusCode = HttpStatusCode.OK;
            var res = "1";
            resp.Content = new StringContent(json, Encoding.UTF8, "application/xml");
            return resp;*/
        }

    }
}
