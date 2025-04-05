using RPMWeb.Data.Common;
using Twilio;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Conversations.V1.Service.Configuration;

namespace RPMWeb.Dal
{
    public class CommServices
    {
        public ChatDetails GenerateChatToken(ChatDetails chatdetails, string UserName, string Application, string CONN_STRING)
        {
            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
            if (lstConfig == null || lstConfig.Count == 0)
            {
                throw new Exception("Invalid System Configurations");
            }
            SystemConfigInfo? Accountsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
            SystemConfigInfo? AuthToken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
            SystemConfigInfo? ApiKey = lstConfig.Find(x => x.Name.Equals("APIKeySID"));
            SystemConfigInfo? ApiSecret = lstConfig.Find(x => x.Name.Equals("APIKeySecrete"));
            SystemConfigInfo? PushCredentialSid;
            switch (Application)
            {
                case "ios":
                    PushCredentialSid = lstConfig.Find(x => x.Name.Equals("PushCredentialAPN"));
                    break;
                case "android":
                    PushCredentialSid = lstConfig.Find(x => x.Name.Equals("PushCredentialSid"));
                    break;
                default:
                    PushCredentialSid = lstConfig.Find(x => x.Name.Equals("PushCredentialSid"));
                    break;
            }
            //SystemConfigInfo PushCredentialSid = lstConfig.Find(x => x.Name.Equals("PushCredentialSid"));
            SystemConfigInfo? ChatServiceSid = lstConfig.Find(x => x.Name.Equals("ChatServiceSid"));

            if (Accountsid == null || AuthToken == null || ApiKey == null || ApiSecret == null || PushCredentialSid == null || ChatServiceSid == null)
            {
                throw new Exception("Invalid System Configurations");
            }

            TwilioClient.Init(Accountsid.Value, AuthToken.Value);

            if (chatdetails.token == null)
            {
                var notification = NotificationResource.Update(
                addedToConversationEnabled: true,
                addedToConversationSound: "default",
                addedToConversationTemplate: "There is a new message in ${CONVERSATION} from ${PARTICIPANT}: ${MESSAGE}",
                pathChatServiceSid: ChatServiceSid.Value
                );
            }

            var grant = new ChatGrant();
            grant.ServiceSid = ChatServiceSid.Value;
            grant.PushCredentialSid = PushCredentialSid.Value;
            var grants = new HashSet<IGrant> { grant };
            var token = new Token(Accountsid.Value, ApiKey.Value, ApiSecret.Value, UserName, grants: grants);
            chatdetails.token = token.ToJwt();
            RpmDalFacade.UpdateChatDetails(UserName, chatdetails);
            return chatdetails;

        }
    }
}
