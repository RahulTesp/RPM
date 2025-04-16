using RPM.Comm.Api;
using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Twilio;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Video.V1;
using ParticipantResource = Twilio.Rest.Video.V1.Room.ParticipantResource;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace RpmCloud.Controllers
{
    [Route("api/comm")]
    public class CommController : ControllerBase
    {
        public readonly string CONN_STRING;
        public CommController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("SMSService")]
        [HttpPost]
        public IActionResult MessagingService([FromBody] SendSmsToPatient Info)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? accsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? authtoken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                    SystemConfigInfo? smssid = lstConfig.Find(x => x.Name.Equals("SMSServiceSID"));
                    SystemConfigInfo? fromPhoneNo = lstConfig.Find(x => x.Name.Equals("MyPhoneNumber"));
                    if (fromPhoneNo == null)
                    {
                        return NotFound("Not Found");
                    }
                    Info.fromPhoneNo = fromPhoneNo.Value;
                    if (accsid != null && authtoken != null && smssid != null)
                    {
                        TwilioCommMgr commMgr = new TwilioCommMgr();
                        
                        
                        if (commMgr.MessagingServiceTopatient(Info, accsid.Value, authtoken.Value, smssid.Value, UserName))
                        {
                            return Ok("Success");
                        }
                    }
                    return NotFound("Not Found");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Route("CallToken")]
        [HttpGet]
        public IActionResult GetCallToken()
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? accsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? twimlappsid = lstConfig.Find(x => x.Name.Equals("TwiMLAppSID"));
                    SystemConfigInfo? apikeysid = lstConfig.Find(x => x.Name.Equals("APIKeySID"));
                    SystemConfigInfo? apisecretkey = lstConfig.Find(x => x.Name.Equals("APIKeySecrete"));
                    SystemConfigInfo? mynumber = lstConfig.Find(x => x.Name.Equals("MyPhoneNumber"));
                    if (accsid != null && twimlappsid != null && apikeysid != null &&
                       apisecretkey != null && mynumber != null)
                    {
                        TwilioCommMgr commMgr = new TwilioCommMgr();
                        AccessToken accT = commMgr.GetCallToken(accsid.Value, twimlappsid.Value,
                                                                apikeysid.Value, apisecretkey.Value,
                                                                mynumber.Value);
                        if (accT != null)
                        {
                            return Ok(accT);
                        }
                    }

                    return NotFound("Not Found");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("CallConnect")]
        [HttpPost]
        public IActionResult CallConnect()
        {
            
            try
            {
                List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                if (lstConfig == null || lstConfig.Count == 0)
                {
                    throw new Exception("Invalid System Configurations");
                }

                SystemConfigInfo? mynumber = lstConfig.Find(x => x.Name.Equals("MyPhoneNumber"));
                if (mynumber == null)
                {
                    throw new Exception("Invalid System Configurations");
                }
                var toNumber = HttpContext.Request.Form["To"];
                if (String.IsNullOrEmpty(toNumber)) { 
                    return BadRequest("Invalid To Number"); 
                }
                TwilioCommMgr commMgr = new TwilioCommMgr();
                string res = commMgr.CallConnect(toNumber, mynumber.Value);
                if (!string.IsNullOrEmpty(res))
                {
                    //resp.StatusCode = HttpStatusCode.OK;
                    //resp.Content = new StringContent(res, Encoding.UTF8, "application/xml");
                    //return resp;
                    return Ok(Content(res, "application/xml", Encoding.UTF8));
                }
                return Unauthorized("Not Found");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("CallRecord")]
        [HttpPost]
        public IActionResult CallRecord()
        {
            var toNumber = HttpContext.Request.Form["RecordingTrack"];
            var res = "1";
            //resp.StatusCode = HttpStatusCode.OK;
            //resp.Content = new StringContent(res, Encoding.UTF8, "application/xml");
            return Ok(Content(res, "application/xml", Encoding.UTF8));
        }
        
        [HttpGet]
        [Route("VideoRoom")]
        public IActionResult CreateVideoRoom(string PatientId,bool IsNotify, bool isCallActive)
        {
            try
            {

                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                   bool PatientOnline = RpmDalFacade.IsPatientOnline(PatientId, UserName);
                   
                    if (PatientOnline) 
                    {
                        commUserNamesforVideoCall commUsrNames = RpmDalFacade.GetCommUserNamesforVideo(UserName, PatientId);
                        List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                        if (lstConfig == null || lstConfig.Count == 0)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        SystemConfigInfo? Accountsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                        SystemConfigInfo? AuthToken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                        SystemConfigInfo? ApiKey = lstConfig.Find(x => x.Name.Equals("APIKeySID"));
                        SystemConfigInfo? ApiSecret = lstConfig.Find(x => x.Name.Equals("APIKeySecrete"));

                        if (Accountsid == null || AuthToken == null || ApiKey == null || ApiSecret == null)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        TwilioClient.Init(Accountsid.Value, AuthToken.Value);

                      
                        VideoCallDetails roomdetails = new VideoCallDetails();

                        roomdetails.room = RpmDalFacade.IsRoomExists(UserName,PatientId);
                        if (roomdetails.room == null)
                        {
                            Guid uuid = Guid.NewGuid();
                            roomdetails.room = commUsrNames.CommUserNameCareTeam+"_"+ commUsrNames.CommUserNamePatient + uuid.ToString();
                            var room = RoomResource.Create(uniqueName: roomdetails.room, maxParticipants: 2);

                            var participant = ParticipantResource.Read(roomdetails.room);

                            RpmDalFacade.UpdateVideoRoom(UserName,PatientId, roomdetails.room);
                        }

                        var participants = ParticipantResource.Read(roomdetails.room);

                        foreach (var participant in participants)
                        {
                            if (participant.Status == ParticipantResource.StatusEnum.Connected)
                            {
                                Console.WriteLine("connected");
                            }
                        }
                        if (!isCallActive)
                        {
                            int bearerId = RpmDalFacade.GetBearerId(s);
                            firebasenotificationmessage notify = new firebasenotificationmessage();
                            GetUserProfiles userProfiles = RpmDalFacade.GetMyProfiles(UserName);
                            notify.title = "Video Call From" + " " + userProfiles.FirstName + " " + userProfiles.LastName;
                            notify.body = roomdetails.room + "@" + bearerId + "#" + isCallActive;
                            string category = "VideoCall";
                            RpmDalFacade.GetFirebaseNotificationByUser(UserName, PatientId, notify, category);
                            Console.WriteLine(notify);
                            return Ok( new { message = "The patient was notified that the call was disconnected." });

                        }
                        var grant = new VideoGrant();
                        grant.Room = roomdetails.room;
                        var grants = new HashSet<IGrant> { grant };
                        var token = new Token(Accountsid.Value, ApiKey.Value, ApiSecret.Value,commUsrNames.CommUserNameCareTeam, grants: grants);
                        roomdetails.token = token.ToJwt();
                        RpmDalFacade.UpdateVideoRoomToken(UserName, roomdetails.token, roomdetails.room);

                        if (IsNotify)
                        {
                            int bearerId = RpmDalFacade.GetBearerId(s);
                            firebasenotificationmessage notify = new firebasenotificationmessage();
                            GetUserProfiles userProfiles = RpmDalFacade.GetMyProfiles(UserName);
                            notify.title = "Video Call From" +" "+ userProfiles.FirstName+" "+userProfiles.LastName;
                            notify.body = roomdetails.room+"@"+bearerId+"#"+ isCallActive;
                            string category = "VideoCall";
                            RpmDalFacade.GetFirebaseNotificationByUser(UserName, PatientId, notify, category);
                            Console.WriteLine(notify);
                           
                        }
                        return Ok(roomdetails);
                   }
                   else
                    {
                        return NotFound("Patient is not Online");
                    }

                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet]
        [Route("joinroom")]
        public IActionResult JoinVideoRoom(string room)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                        List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                        if (lstConfig == null || lstConfig.Count == 0)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        SystemConfigInfo? Accountsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                        SystemConfigInfo? AuthToken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                        SystemConfigInfo? ApiKey = lstConfig.Find(x => x.Name.Equals("APIKeySID"));
                        SystemConfigInfo? ApiSecret = lstConfig.Find(x => x.Name.Equals("APIKeySecrete"));

                        if (Accountsid == null || AuthToken == null || ApiKey == null || ApiSecret == null)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        TwilioClient.Init(Accountsid.Value, AuthToken.Value);
                        VideoCallDetails roomdetails = new VideoCallDetails();
                        roomdetails.room = room;
                        var grant = new VideoGrant();
                        grant.Room = roomdetails.room;
                        var grants = new HashSet<IGrant> { grant };
                        string commUserName= RpmDalFacade.GetCommUserName(UserName);
                        var token = new Token(Accountsid.Value, ApiKey.Value, ApiSecret.Value, commUserName, grants: grants);
                        roomdetails.token = token.ToJwt();
                        RpmDalFacade.UpdateVideoRoomToken(UserName, roomdetails.token, roomdetails.room);
                        return Ok(roomdetails.token);
                    }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        [Route("getchattoken")]
        public IActionResult CreateChat(string app)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);

                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    ChatDetails chatdetails = new ChatDetails();
                    chatdetails.istoken = false;
                    chatdetails = RpmDalFacade.GetChatDetails(UserName);

                    if (!chatdetails.istoken)
                    {
                        chatdetails.makeisactivezero = false;
                        chatdetails = RpmDalFacade.GenerateChatToken(chatdetails, UserName,app);

                    }
                    return Ok( new { message = chatdetails.token });
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("regeneratechattoken")]
        public IActionResult RegenerateChatToken(string app)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);

                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    ChatDetails chatdetails = new ChatDetails();
                    chatdetails = RpmDalFacade.GetChatDetails(UserName);
                    chatdetails.makeisactivezero = true;
                    chatdetails = RpmDalFacade.GenerateChatToken(chatdetails, UserName, app);
                    return Ok(chatdetails.token);
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
                }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("updatechatresource")]
        public IActionResult UpdateChatResource([FromBody] ChatResourceDetails chatresource)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    chatresource.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    bool ischatresurceupdated = RpmDalFacade.UpdateChatResource(chatresource);
                    if(ischatresurceupdated)
                    {
                        return Ok();
                    }
                    else
                    {
                        return NotFound("Could not save chat details");
                    }
                    
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("getchatsid")]
        public IActionResult GetChatResource(string ToUser)
        {
            
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }     
                    string ConversationSid = RpmDalFacade.GetChatResource(UserName, ToUser);
                    if (ConversationSid==null)
                    {
                        return NotFound("No Conversation History Found");
                        
                    }
                    return Ok(ConversationSid);

                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            
        }
        [HttpGet]
        [Route("getallchats")]
        public IActionResult GetAllChats(string ToUser)
        {

            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConf == null || lstConf.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? AccountSID = lstConf.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? AuthToken = lstConf.Find(x => x.Name.Equals("AuthToken"));
                    if(AuthToken==null || AccountSID== null)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    string AccountSIDValue = Convert.ToString(AccountSID.Value);
                    string AuthTokenValue = Convert.ToString(AuthToken.Value);

                    List<ConverationHistory> Conversations = RpmDalFacade.GetAllConversations(UserName, ToUser, AccountSIDValue, AuthTokenValue);
                    if (Conversations.Count == 0)
                    {
                        return NotFound("No Conversation History Found");

                    }
                    return Ok(Conversations);

                }
                else
                {
                    return Unauthorized("Invalid session.");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpPost]
        [Route("notifibyfirebase")]

        public IActionResult NotificationbyFirebase([FromBody] string toUser, int tokenid, firebasenotificationmessage notify)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                        string category = "Rejection";
                        RpmDalFacade.GetFirebaseNotificationCallRejection(UserName, toUser, notify, category,tokenid);
                        return Ok("Notification Sent Successfully");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [HttpPost]
        [Route("updatechatwebhook")]
        public IActionResult UpdateChatWebhook([FromBody] chathook hook)
        {
            try
            {
                bool ischatresurceupdated = RpmDalFacade.UpdateChatWebhook(hook);
                if (ischatresurceupdated)
                {
                    return Ok();
                }
                else
                {
                    return NotFound("");
                }               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("updatesmswebhook")]
        public IActionResult UpdatesmsWebhook([FromBody] smshook hook)
        {
            try
            {
                RpmDalFacade.UpdateIncomingSmsDetails(hook);
                return Ok();
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("chatheartbeat")]
        public IActionResult UpdateUserConversationActivity([FromBody] ConversationHeartBeat conv)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    bool isUpdated = RpmDalFacade.UpdateUserConversationActivity(UserName, conv.ConversationSid, conv.LastActiveAt, conv.UserName);
                    if (isUpdated)
                    {
                        return Ok("Activity updated successfully.");
                    }
                    else
                    {
                        return NotFound("Failed to update activity.");
                    }
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("NotifyConversation")]
        public IActionResult GetActiveUserCountLastMinute(ConversationsNotification conv)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    RpmDalFacade.NotifyConversation(conv.ConversationSid, conv.FromUser, conv.ToUser);
                    return Ok();
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}









