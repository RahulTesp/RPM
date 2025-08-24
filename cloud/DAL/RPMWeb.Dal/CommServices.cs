using RPMWeb.Data.Common;
using System.Text.Json;
using System.Text;
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
            chatdetails.Application = Application;
            RpmDalFacade.UpdateChatDetails(UserName, chatdetails);
            return chatdetails;

        }
        public DateTime? GetExpiryFromJwt(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return null;

            var parts = jwtToken.Split('.');
            if (parts.Length != 3)
                return null;

            // Decode the payload
            string payload = parts[1];
            payload = PadBase64(payload);
            var jsonBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(jsonBytes);

            // Parse payload
            using (JsonDocument doc = JsonDocument.Parse(payloadJson))
            {
                if (doc.RootElement.TryGetProperty("exp", out JsonElement expElement))
                {
                    long expUnix = expElement.GetInt64();
                    // Convert Unix timestamp to DateTime
                    DateTimeOffset expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                    return expDate.UtcDateTime;
                }
            }

            return null;
        }
        private static string PadBase64(string base64)
        {
            int pad = 4 - base64.Length % 4;
            if (pad < 4)
            {
                base64 += new string('=', pad);
            }
            return base64.Replace('-', '+').Replace('_', '/');
        }
    }
}
