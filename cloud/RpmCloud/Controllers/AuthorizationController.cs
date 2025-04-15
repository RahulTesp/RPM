using RPM.Comm.Api;
using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        public readonly string CONN_STRING;
        public AuthorizationController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("verifyusername")]
        [HttpPost]
        public IActionResult VerifyUserName([FromBody]VerifyUserName verifyUserName)
        {            
            RpmDalFacade.ConnectionString = CONN_STRING;
            if (RpmDalFacade.VerifyUserName(verifyUserName))
            {
                return Ok();
            }
            return NotFound("");     
        }

        [Route("Userlogin")]
        [HttpPost]
        public IActionResult Login(RPMLogin verPass)
        {
            try
            {
                bool otprequired = false;
                RpmDalFacade.ConnectionString = CONN_STRING;
                if (verPass.UserName == null)
                {
                    return Unauthorized("Unauthorized user.");
                }
                bool isActive = RpmDalFacade.CheckUserActive(verPass.UserName);

                if(!isActive)
                {
                    return Unauthorized("Unauthorized user.");
                }

                UserRoleConfig resp = RpmDalFacade.GetUserRoleConfig(verPass.UserName);
                if(resp!= null)
                {
                    if(resp.IsMailSend||resp.IsSmsSend)
                    {
                        otprequired = true;
                    }
                }
                List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Login", "User");
                if (lstConf == null || lstConf.Count == 0)
                {
                    throw new Exception("Invalid System Configurations");
                }
                SystemConfigInfo? Mfa = lstConf.Find(x => x.Name.Equals("MFA"));
                SystemConfigInfo? MfaLimit = lstConf.Find(x => x.Name.Equals("MFATimeOut"));
                SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                if(Mfa == null || MfaLimit == null || MfaRetryCount == null)
                {
                    throw new Exception("Invalid System Configurations");
                }
                int MFA_Enable = Convert.ToInt32(Mfa.Value);
                if (MFA_Enable!=0 && otprequired)
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);
                    if (UserLocked)
                    {
                        return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                    }
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = RpmDalFacade.GetPhoneNumberByUserName(verPass.UserName);
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? accsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? authtoken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                    SystemConfigInfo? smssid = lstConfig.Find(x => x.Name.Equals("SMSServiceSID"));
                    var response = RpmDalFacade.Login(verPass);
                    int myRandomNo = 0;
                    LoginDetails? loginDetails = new LoginDetails();
                    LoginResponseToken loginResponseToken = new LoginResponseToken();
                    if (response.tkn!=null)
                    {
                        //otp
                        Random rnd = new Random();
                        myRandomNo = rnd.Next(10000000, 99999999);
                        //new token
                        loginResponseToken = RpmDalFacade.CreateNewToken(Convert.ToInt32(MfaLimit.Value));
                        //insert otp and new token to logindetails table
                        loginDetails = RpmDalFacade.InsertLoginDetails(verPass.UserName, Convert.ToString(myRandomNo), loginResponseToken.tkn);
                    }
                    else
                    {
                        loginDetails = null;
                    }

                    string phonenumber = string.Empty;
                    bool msgSent = false;
                    bool ValidMailId = false;
                    bool validMobileNumber = false;
                    if (loginDetails !=null)
                    {
                        List<SystemConfigInfo> providerName = DalCommon.GetSystemConfig(CONN_STRING, "Provider", "User");
                        if (lstConf == null || lstConf.Count == 0)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        SystemConfigInfo? provider = providerName.Find(x => x.Name.Equals("Provider"));
                        if(resp== null)
                        {
                            return BadRequest("Invalid User");
                        }
                        if (resp.IsMailSend && resp.IsSmsSend)
                        {
                            if(provider== null)
                            {
                                return BadRequest("Invalid Provider");
                            }
                            //xlawywleciknobww
                            string fromEmail = contactDetails.FromMail;
                            string toEmail = contactDetails.MailId;
                            string subject = "One Time Password";
                            string body = "Please use this OTP : "+myRandomNo+" to log into "+provider.Value+"'s"+" RPM portal,Do not share it with anyone";
                            SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                            smtpClient.EnableSsl = true;
                            smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                            MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                            try
                            {
                                smtpClient.Send(mailMessage);
                                ValidMailId = true;
                            }
                            catch (Exception ex)
                            {
                                ValidMailId=false;
                            }

                            try
                            {
                                if (accsid != null && authtoken != null && smssid != null)
                                {
                                    List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                    SystemConfigInfo? Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                    TwilioCommMgr commMgr = new TwilioCommMgr();
                                    SendSms sendSms = new SendSms();
                                    if(Code == null)
                                    {
                                        return BadRequest("Invalid Country Code");
                                    }
                                    sendSms.PhoneNo = Code.Value+contactDetails.MobileNumber;
                                    phonenumber = contactDetails.MobileNumber;
                                    sendSms.Message = "Please use this OTP : "+myRandomNo+" to log into "+provider.Value+"'s"+" RPM portal,Do not share it with anyone";
                                    msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                    validMobileNumber = true;
                                }
                            }
                            catch (Exception)
                            {
                                validMobileNumber=false;
                            }

                        }
                        else if(resp.IsMailSend)
                        {
                            // xlawywleciknobww
                            try
                            {
                                if (provider == null) { return BadRequest("Invalid Provider"); }
                                string fromEmail = contactDetails.FromMail;
                                string toEmail = contactDetails.MailId;
                                string subject = "One Time Password";
                                string body = "Please use this OTP : "+myRandomNo+" to log into "+provider.Value+"'s"+" RPM portal,Do not share it with anyone";
                                SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                                smtpClient.EnableSsl = true;
                                smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                                MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                                smtpClient.Send(mailMessage);
                                ValidMailId=true;

                            }
                            catch (Exception ex)
                            {
                                return StatusCode(503, "Failed to send otp to Email,Please contact Admin");
                            }
                        }
                        else if(resp.IsSmsSend)
                        {
                            try
                            {
                                if (accsid != null && authtoken != null && smssid != null)
                                {
                                    List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                    SystemConfigInfo? Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                    TwilioCommMgr commMgr = new TwilioCommMgr();
                                    SendSms sendSms = new SendSms();
                                    if(Code == null)
                                    {
                                        return BadRequest("Invalid Country Code");
                                    }
                                    if(provider == null)
                                    {
                                        return BadRequest("Invalid Provider");
                                    }
                                    sendSms.PhoneNo = Code.Value+contactDetails.MobileNumber; ;
                                    phonenumber = contactDetails.MobileNumber; ;
                                    sendSms.Message = "Please use this OTP : "+myRandomNo+" to log into "+provider.Value+"'s"+" RPM portal,Do not share it with anyone";
                                    msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                    validMobileNumber=true;
                                }
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(503, "Failed to send otp to Mobile,Please contact Admin");
                            }
                        }
                        //send otp  to patient mobile
                    }
                    if (response.tkn == null)
                    {
                        int RetryAdded = RpmDalFacade.UpdateRetryCount(verPass.UserName);
                        if (RetryAdded>0)
                        {
                            if (RetryAdded==Convert.ToInt32(MfaRetryCount.Value))
                            {
                                return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                            }
                            else
                            {
                                return Unauthorized("Invalid Login details, Please check the username/passowrd.");
                            }
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        List<Roles> roles = new List<Roles>();
                        roles = RpmDalFacade.GetUserRoles(verPass.UserName);
                        loginResponseToken.Roles = roles;
                        loginResponseToken.MFA=true;
                        loginResponseToken.TimeLimit =Convert.ToInt32(MfaLimit.Value);
                        string input = contactDetails.MailId;
                        string pattern = @"(?<=[\w]{1})[\w\-._\+%]*(?=[\w]{1}@)";
                        string result = Regex.Replace(input, pattern, m => new string('*', m.Length));
                        if(resp==null)
                        {
                            return BadRequest("Invalid User");
                        }
                        if (resp.IsMailSend&&resp.IsSmsSend)
                        {
                            loginResponseToken.Mobilenumber="xxxxxxxx"+phonenumber.Substring(phonenumber.Length - 2);
                            loginResponseToken.MailId=result;
                            loginResponseToken.ValidMailId=ValidMailId;
                            loginResponseToken.ValidMobile=validMobileNumber;
                        }
                        else if(resp.IsMailSend)
                        {
                            loginResponseToken.MailId=result;
                            loginResponseToken.ValidMailId=ValidMailId;
                            loginResponseToken.ValidMobile=validMobileNumber;

                        }
                        else if(resp.IsSmsSend)
                        {
                            loginResponseToken.Mobilenumber="xxxxxxxx"+phonenumber.Substring(phonenumber.Length - 2);
                            loginResponseToken.ValidMailId=ValidMailId;
                            loginResponseToken.ValidMobile=validMobileNumber;
                        }


                        if (loginResponseToken.tkn!=null)
                        {
                            return Ok(loginResponseToken);
                        }
                        else
                        {
                            return Unauthorized(loginResponseToken);
                        }
                    }
                }

                else
                {
                    if(MFA_Enable==0)
                    {
                        bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);
                        if (UserLocked)
                        {
                            return Forbid("The User is locked, Please contact your careteam.");
                        }
                    }
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    var response = RpmDalFacade.Login(verPass);
                    response.MFA=false;
                    if (response.tkn == null)
                    {
                        int RetryAdded = RpmDalFacade.UpdateRetryCount(verPass.UserName);

                        if (RetryAdded>0)
                        {
                            if (RetryAdded==Convert.ToInt32(MfaRetryCount.Value))
                            {
                                return Forbid("The User is locked, Please contact your careteam.");
                            }
                            else
                            {
                                return Unauthorized("Invalid Login details, Please check the username/passowrd.");
                            }

                        }
                        return Unauthorized();
                    }
                    else
                    {
                        //update login details table to reset the retry count and lock mechanism
                        int ret = RpmDalFacade.UpdateLoginDetails(verPass.UserName);
                        if(ret!=0)
                        {
                            return Ok(response);
                        }
                        return StatusCode(500, "Db update failed");
                    }
                } 
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
                               
        }
        [Route("UserloginVerifiy")]
        [HttpPost]
        public IActionResult LoginWithOtp(RPMWeb.Data.Common.Login verPass)
        {
            try
            {
                LoginDetails loginDetails = new LoginDetails(); 
                LoginResponseToken token =  new LoginResponseToken();
                RpmDalFacade.ConnectionString = CONN_STRING;
                string? tok = string.Empty;
                if(Request.Headers.ContainsKey("Bearer"))
                {
                    tok = Request.Headers["Bearer"].FirstOrDefault();
                    if (string.IsNullOrEmpty(tok))
                    {
                        return Unauthorized("Invalid session.");
                    }
                }
                bool sessionValid = RpmDalFacade.IsNewSessionValid(tok, verPass.UserName);
                if (sessionValid)
                {
                    bool tokenValid = RpmDalFacade.ValidateTkn(tok);
                    if(tokenValid)
                    {
                        loginDetails =  RpmDalFacade.VerifyOtp(verPass.OTP, verPass.UserName);
                        if(loginDetails != null)
                        {
                            List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Login", "User");
                            if (lstConf == null || lstConf.Count == 0)
                            {
                                throw new Exception("Invalid System Configurations");
                            }
                            SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                            if (loginDetails.Match.ToLower()=="nomatch" || loginDetails.RetryCount>Convert.ToInt32(MfaRetryCount.Value))
                            {
                                if(loginDetails.RetryCount>=Convert.ToInt32(MfaRetryCount.Value))
                                {
                                    bool resp = RpmDalFacade.LockUser(verPass.UserName);
                                    if (resp)
                                    {
                                        return Forbid("User Locked,Please contact your careteam");
                                    }
                                    else
                                    {
                                        return Forbid("User lock update failed.");
                                    }
                                }
                                else
                                {
                                    return Unauthorized("Invalid OTP");
                                }
                            }

                            else
                            {
                                token =  RpmDalFacade.GetSessionByUserName(verPass.UserName);
                                DateTime? lastPwdChange = RpmDalFacade.GetLastPswdChange(verPass.UserName);
                                if(lastPwdChange != null)
                                {
                                    DateTime newenddate = Convert.ToDateTime(lastPwdChange).AddDays(30);
                                    if (newenddate >= DateTime.Now)
                                    {
                                        token.reqPasswordchange = false;
                                    }
                                    else
                                    {
                                        token.reqPasswordchange = false;
                                    }
                                }
                                List<Roles> roles = new List<Roles>();
                                roles = RpmDalFacade.GetUserRoles(verPass.UserName);
                                if(roles != null)
                                {
                                    token.Roles=roles;
                                }
                                if (token.tkn!=null && token.tkn.Length>0)
                                {
                                    token.tkt="Bearer";
                                    return Ok(token);
                                }
                                else
                                {
                                    return Unauthorized();
                                }
                            }
                        }
                        else
                        {
                            return Forbid("OTP validation process failed.");
                        }
                        
                        
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [Route("userresetpasswordverifiy")]
        [HttpPost]
        public IActionResult ResetWithOtp(ResetPassword verPass)
        {
            try
            {
                LoginDetails loginDetails = new LoginDetails();
                LoginResponseToken token = new LoginResponseToken();
                RpmDalFacade.ConnectionString = CONN_STRING;
                string? tok = string.Empty;
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    tok = Request.Headers["Bearer"].FirstOrDefault();
                    if (string.IsNullOrEmpty(tok))
                    {
                        return Unauthorized("Invalid session.");
                    }
                }
                bool sessionValid = RpmDalFacade.IsNewSessionValid(tok, verPass.Username);
                if (sessionValid)
                {
                    bool tokenValid = RpmDalFacade.ValidateTkn(tok);
                    if (tokenValid)
                    {
                        loginDetails =  RpmDalFacade.VerifyOtp(verPass.Otp, verPass.Username);
                        if (loginDetails != null)
                        {
                            List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Login", "User");
                            if (lstConf == null || lstConf.Count == 0)
                            {
                                throw new Exception("Invalid System Configurations");
                            }
                            SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                            if (loginDetails.Match.ToLower()=="nomatch" || loginDetails.RetryCount>Convert.ToInt32(MfaRetryCount.Value))
                            {
                                if (loginDetails.RetryCount>=Convert.ToInt32(MfaRetryCount.Value))
                                {
                                    bool resp = RpmDalFacade.LockUser(verPass.Username);
                                    if (resp)
                                    {
                                        return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                                    }
                                    else
                                    {
                                        return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                                    }
                                }
                                else
                                {
                                    return Unauthorized("OTP entered is not valid.");
                                }
                            }
                            else
                            {  
                                bool res = RpmDalFacade.UpdateUserPassword(verPass.Username, verPass.password);
                                if (res)
                                {
                                    bool res1 = RpmDalFacade.ClearOldSessions(verPass.Username);
                                    if (res1)
                                    {
                                        return Ok("Password changed successfully, please re-login to continue.");
                                    }
                                    else
                                    {
                                        return Ok("Password changed successfully, old sessions are still alive and please re-login to continue.");
                                    }
                                }
                                else
                                {
                                    return Forbid("Failed to Reset Password, Please try again.");
                                }
                            }
                        }
                        else
                        {
                            return Forbid("OTP entered is not valid.");
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [Route("forgetpassword")]
        [HttpGet]
        public IActionResult ForgetPasswordOtp(string username)
        {
            try
            {
                RpmDalFacade.ConnectionString = CONN_STRING;
                bool UserLocked = RpmDalFacade.CheckUserLockedStatus(username);
                if (UserLocked)
                {
                    return Forbid("The User is locked, Please contact your careteam.");
                }
                bool isActive = RpmDalFacade.CheckUserActive(username);

                if (!isActive)
                {
                    return NotFound("The User is Inactive");
                }
                LoginResponseToken token = new LoginResponseToken();
                RpmDalFacade.ConnectionString = CONN_STRING;
                string tok = string.Empty;
                RpmDalFacade.ConnectionString = CONN_STRING;
                Random rnd = new Random();
                int myRandomNo = 0;
                myRandomNo = rnd.Next(10000000, 99999999);
                //new token
                List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Login", "User");
                if (lstConf == null || lstConf.Count == 0)
                {
                    throw new Exception("Invalid System Configurations");
                }
                UserRoleConfig resp = RpmDalFacade.GetUserRoleConfig(username);
                bool ValidMailId = false;
                bool validMobileNumber = false;
                bool otprequired = false;
                if (resp.IsMailSend||resp.IsSmsSend)
                {
                    otprequired = true;
                }
                SystemConfigInfo? MfaLimit = lstConf.Find(x => x.Name.Equals("ResetOtpExpiry"));
                LoginResponseToken loginResponseToken = new LoginResponseToken();
                if(MfaLimit == null)
                {
                    throw new Exception("Invalid System Configurations");
                }
                loginResponseToken = RpmDalFacade.CreateNewToken(Convert.ToInt32(MfaLimit.Value));
                //insert otp and new token to logindetails table
                bool unlock = RpmDalFacade.UnlockUserByUsername(username);
                LoginDetails loginDetails = new LoginDetails();
                loginDetails = RpmDalFacade.InsertLoginDetails(username, Convert.ToString(myRandomNo), loginResponseToken.tkn);
                // bool res = RpmDalFacade.UnlockUser(unlockUser.UserId, unlockUser.Patientid);
                if (loginDetails!=null && resp!= null && otprequired)
                {
                    bool msgSent = false;
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = RpmDalFacade.GetPhoneNumberByUserName(username);
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? accsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? authtoken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                    SystemConfigInfo? smssid = lstConfig.Find(x => x.Name.Equals("SMSServiceSID"));
                    if (resp.IsMailSend && resp.IsSmsSend)
                    {
                        //xlawywleciknobww
                        string fromEmail = contactDetails.FromMail;
                        string toEmail = contactDetails.MailId;
                        string subject = "One Time Password";
                        string body = "Please use this OTP : "+myRandomNo+" to reset password ,Do not share it with anyone";
                        SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                        MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                        try
                        {
                            smtpClient.Send(mailMessage);
                            ValidMailId = true;
                        }
                        catch (Exception ex)
                        {
                            ValidMailId=false;
                        }

                        try
                        {
                            if (accsid != null && authtoken != null && smssid != null)
                            {
                                List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                SystemConfigInfo Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                TwilioCommMgr commMgr = new TwilioCommMgr();
                                SendSms sendSms = new SendSms();
                                // sendSms.PhoneNo = "+910000000";
                                sendSms.PhoneNo = Code.Value+contactDetails.MobileNumber;
                                //phonenumber = contactDetails.MobileNumber;
                                sendSms.Message = "Please use this OTP : "+myRandomNo+" to reset password,Do not share it with anyone.";
                                msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                validMobileNumber = true;
                            }
                        }

                        catch (Exception ex)
                        {
                            validMobileNumber=false;
                        }

                    }
                    else if (resp.IsMailSend)
                    {
                        // xlawywleciknobww
                        try
                        {
                            string fromEmail = contactDetails.FromMail;
                            string toEmail = contactDetails.MailId;
                            string subject = "OTP";
                            string body = "Please use this OTP : "+myRandomNo+" to reset password,Do not share it with anyone";
                            SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                            smtpClient.EnableSsl = true;
                            smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                            MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                            smtpClient.Send(mailMessage);
                            ValidMailId=true;

                        }
                        catch (Exception ex)
                        {
                            return StatusCode(503,"Failed to send otp to Email,Please contact Admin");
                        }
                    }
                    else if (resp.IsSmsSend)
                    {
                        try
                        {
                            if (accsid != null && authtoken != null && smssid != null)
                            {
                                List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                SystemConfigInfo? Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                TwilioCommMgr commMgr = new TwilioCommMgr();
                                SendSms sendSms = new SendSms();
                                sendSms.PhoneNo = Code.Value+contactDetails.MobileNumber;
                                sendSms.Message = "Please use this OTP : "+myRandomNo+" to reset password,Do not share it with anyone.";
                                msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                validMobileNumber=true;

                            }
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(503, "Failed to send otp to Mobile,Please contact Admin");
                        }
                    }
                    loginResponseToken.MFA=true;
                    loginResponseToken.TimeLimit =Convert.ToInt32(MfaLimit.Value);
                    string input = contactDetails.MailId;
                    string pattern = @"(?<=[\w]{1})[\w\-._\+%]*(?=[\w]{1}@)";
                    string result = Regex.Replace(input, pattern, m => new string('*', m.Length));
                    if (resp.IsMailSend&&resp.IsSmsSend)
                    {
                        loginResponseToken.Mobilenumber="xxxxxxxx"+contactDetails.MobileNumber.Substring(contactDetails.MobileNumber.Length - 2);
                        loginResponseToken.MailId=result;
                        loginResponseToken.ValidMailId=ValidMailId;
                        loginResponseToken.ValidMobile=validMobileNumber;
                    }
                    else if (resp.IsMailSend)
                    {
                        loginResponseToken.MailId=result;
                        loginResponseToken.ValidMailId=ValidMailId;
                        loginResponseToken.ValidMobile=validMobileNumber;

                    }
                    else if (resp.IsSmsSend)
                    {
                        loginResponseToken.Mobilenumber="xxxxxxxx"+contactDetails.MobileNumber.Substring(contactDetails.MobileNumber.Length - 2);
                        loginResponseToken.ValidMailId=ValidMailId;
                        loginResponseToken.ValidMobile=validMobileNumber;
                    }
                    return Ok(loginResponseToken);
                }
                return NotFound("Invalid Login details, Please check the username.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("UnlockUser")]
        [HttpPost]
        public IActionResult UnlockUser(UnlockUser unlockUser)
        {
            try
            {
                LoginDetails loginDetails = new LoginDetails();
                LoginResponseToken token = new LoginResponseToken();
                RpmDalFacade.ConnectionString = CONN_STRING;
                string? tok = string.Empty;
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    tok = Request.Headers["Bearer"].FirstOrDefault();
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    if (string.IsNullOrEmpty(tok))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(tok);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(tok))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    bool res = RpmDalFacade.UnlockUser(unlockUser.UserId,unlockUser.Patientid);
                    if (res)
                    {
                        return Ok("User/Patient Unlocked");
                    }
                    return NotFound("Could not find  details");
                }
                else
                {
                    return  Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                RpmDalFacade.ConnectionString = CONN_STRING;
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
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
                    Logout logout = new Logout();
                    logout.JwtToken = s;
                    logout.createdBy = UserName;
                    if (RpmDalFacade.LogOut(logout))
                    {
                        return Ok("Logout Sucess");
                    }
                    return NotFound("Invalid session");
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
        [Route("updatepassword")]
        [HttpPost]
        public IActionResult UpdatePassword(Updatepassword updatepassword)
        {
            StatusMessage msg = new StatusMessage();
            try
            {
                RpmDalFacade.ConnectionString = CONN_STRING;
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
                    if (string.IsNullOrEmpty(s))
                    {
                        msg.Status = "Invalid session.";
                        return Unauthorized(msg);
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        msg.Status = "Invalid session.";
                        return Unauthorized(msg);
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        msg.Status = "Invalid session.";
                        return Unauthorized(msg);
                    }

                    if (RpmDalFacade.UpdatePassword(updatepassword))
                    {
                        msg.Status = "Password Updated";
                        return Ok(msg);
                    }
                    msg.Status = "Invalid Password";
                    return NotFound(msg);
                }
                else
                {
                    msg.Status = "Invalid session.";
                    return Unauthorized(msg);
                }
            }
            catch (Exception ex)
            {
                msg.Status = ex.Message;
                return BadRequest(msg);
            }
        }
        [Route("operationalmasterdata")]
        [HttpGet]
        public IActionResult OperationalMasterData()
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
                        return Unauthorized( "Invalid session.");
                    }

                    List<OperationalMasterData> Info = RpmDalFacade.GetOperationalMasterData(UserName);
                    if (!(Info == null))
                    {
                        return Ok(Info);
                    }
                    return NotFound("Could not find  details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest("Invalid session.");
                }
                return BadRequest("Unexpected Error.");
            }
        }
        [Route("rolesmasterdata")]
        [HttpGet]
        public IActionResult RolesMasterData()
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

                    List<Roles> Info = RpmDalFacade.GetRolesMasterData(UserName);
                    if (!(Info == null))
                    {
                        return Ok(Info);
                    }
                    return NotFound("Could not find  details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest("Invalid session.");
                }
                return BadRequest("Unexpected Error.");
            }
        }
        [Route("masterdatastatesandcities")]
        [HttpGet]
        public IActionResult masterdatastatesandcities()
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
                    DataSet Info = RpmDalFacade.GetMasterDataForStatesAndCities(UserName);
                    if (!(Info == null))
                    {
                        return Ok(JsonConvert.SerializeObject(Info, Formatting.Indented));
                    }
                    return NotFound("Could not find  details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest("Invalid session.");
                }
                return BadRequest("Unexpected Error.");
            }
        }
        [Route("useraccessrights")]
        [HttpGet]
        public IActionResult GetUserAccessRights([FromQuery] int RoleId)
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
                    UserPermission up = RpmDalFacade.GetUserAccessRights(UserName, RoleId);
                    if (up == null)
                    {
                        return BadRequest();
                    }
                    return Ok(up);
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest("Invalid session.");
                }
                return BadRequest("Unexpected Error.");
            }
        }
        //[Route("connecthub")]
        //[HttpGet]
        //public IActionResult ConnectNotificationHub()
        //{
        //    try
        //    {
        //        RpmDalFacade.ConnectionString = CONN_STRING;
        //        if (Request.Headers.ContainsKey("Bearer"))
        //        {
        //            string? s = Request.Headers["Bearer"].FirstOrDefault();
        //            if (string.IsNullOrEmpty(s))
        //            {
        //                return Unauthorized("Invalid session.");
        //            }
        //            string UserName = RpmDalFacade.IsSessionValid(s);
        //            if (string.IsNullOrEmpty(UserName))
        //            {
        //                return Unauthorized("Invalid session.");
        //            }
        //            if (!RpmDalFacade.ValidateTkn(s))
        //            {
        //                return Unauthorized("Invalid session.");
        //            }
        //            List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Notify", "User");
        //            if (lstConfig == null || lstConfig.Count == 0)
        //            {
        //                throw new Exception("Invalid system config for notifications");
        //            }
        //            SystemConfigInfo? systemConfigInfo = lstConfig.Find(x => x.Name.Equals("PubSubKey"));
        //            if (systemConfigInfo == null) throw new Exception("Invalid system config for notifications");
        //            SystemConfigInfo? _hubnameinfo = lstConfig.Find(x => x.Name.Equals("HubName"));
        //            if (_hubnameinfo == null) throw new Exception("Invalid system config for notifications");
        //            var connString = systemConfigInfo.Value;
        //            var hubname = _hubnameinfo.Value;
        //            return Ok("hubname");
        //        }
        //        else
        //        {
        //            return Unauthorized("Invalid session.");
        //        }
        //    }
        //    catch
        //    {
        //        return BadRequest("Unexpected Error.");
        //    }
        //}
        [Route("Patientlogin")]
        [HttpPost]
        public IActionResult AppPatientLogin(RPMWeb.Data.Common.RPMLogin verPass)
        {
            try
            {
                bool otprequired = false;
                RpmDalFacade.ConnectionString = CONN_STRING;

                bool isActive = RpmDalFacade.CheckPatientActive(verPass.UserName);

                if (!isActive)
                {
                    return Unauthorized("Unauthorized user.");
                }

                UserRoleConfig resp = RpmDalFacade.GetUserRoleConfig(verPass.UserName);
                if (resp != null)
                {
                    if (resp.IsMailSend || resp.IsSmsSend)
                    {
                        otprequired = true;
                    }
                }
                List<SystemConfigInfo> lstConf = DalCommon.GetSystemConfig(CONN_STRING, "Login", "User");
                if (lstConf == null || lstConf.Count == 0)
                {
                    throw new Exception("Invalid System Configurations");
                }
                SystemConfigInfo? Mfa = lstConf.Find(x => x.Name.Equals("MFA"));
                SystemConfigInfo? MfaLimit = lstConf.Find(x => x.Name.Equals("MFATimeOut"));
                SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                if (Mfa == null)
                {
                    throw new Exception("Something went wrong!");
                }
                int MFA_Enable = Convert.ToInt32(Mfa.Value);
                if (MFA_Enable != 0 && otprequired)
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);
                    if (UserLocked)
                    {
                        return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                    }
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = RpmDalFacade.GetPhoneNumberByUserName(verPass.UserName);
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        throw new Exception("Invalid System Configurations");
                    }
                    SystemConfigInfo? accsid = lstConfig.Find(x => x.Name.Equals("AccountSID"));
                    SystemConfigInfo? authtoken = lstConfig.Find(x => x.Name.Equals("AuthToken"));
                    SystemConfigInfo? smssid = lstConfig.Find(x => x.Name.Equals("SMSServiceSID"));
                    var response = RpmDalFacade.Login(verPass);
                    int myRandomNo = 0;
                    LoginDetails loginDetails = new LoginDetails();
                    LoginResponseToken loginResponseToken = new LoginResponseToken();
                    if (response.tkn != null)
                    {
                        //otp
                        Random rnd = new Random();
                        myRandomNo = rnd.Next(10000000, 99999999);
                        //new token
                        loginResponseToken = RpmDalFacade.CreateNewToken(Convert.ToInt32(MfaLimit.Value));
                        //insert otp and new token to logindetails table
                        loginDetails = RpmDalFacade.InsertLoginDetails(verPass.UserName, Convert.ToString(myRandomNo), loginResponseToken.tkn);
                    }
                    else
                    {
                        loginDetails = null;
                    }
                    string phonenumber = string.Empty;
                    bool msgSent = false;
                    bool ValidMailId = false;
                    bool validMobileNumber = false;
                    if (loginDetails != null)
                    {
                        List<SystemConfigInfo> providerName = DalCommon.GetSystemConfig(CONN_STRING, "Provider", "User");
                        if (lstConf == null || lstConf.Count == 0)
                        {
                            throw new Exception("Invalid System Configurations");
                        }
                        SystemConfigInfo? provider = providerName.Find(x => x.Name.Equals("Provider"));
                        if (resp == null || provider == null)
                        {
                            throw new Exception("Something went wrong!");
                        }
                        if (resp.IsMailSend && resp.IsSmsSend)
                        {
                            //xlawywleciknobww
                            string fromEmail = contactDetails.FromMail;
                            string toEmail = contactDetails.MailId;
                            string subject = "One Time Password";
                            string body = "Please use this OTP : " + myRandomNo + " to log into " + provider.Value + "'s" + " RPM portal,Do not share it with anyone";
                            SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                            smtpClient.EnableSsl = true;
                            smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                            MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                            try
                            {
                                smtpClient.Send(mailMessage);
                                ValidMailId = true;
                            }
                            catch (Exception ex)
                            {
                                ValidMailId = false;
                            }
                            try
                            {
                                if (accsid != null && authtoken != null && smssid != null)
                                {
                                    List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                    SystemConfigInfo? Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                    TwilioCommMgr commMgr = new TwilioCommMgr();
                                    SendSms sendSms = new SendSms();
                                    // sendSms.PhoneNo = "+910000000";
                                    sendSms.PhoneNo = Code.Value + contactDetails.MobileNumber;
                                    phonenumber = contactDetails.MobileNumber;
                                    sendSms.Message = "Please use this OTP : " + myRandomNo + " to log into " + provider.Value + "'s" + " RPM portal,Do not share it with anyone";

                                    msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                    validMobileNumber = true;


                                }
                            }

                            catch (Exception ex)
                            {

                                validMobileNumber = false;


                            }

                        }
                        else if (resp.IsMailSend)
                        {
                            // xlawywleciknobww
                            try
                            {
                                string fromEmail = contactDetails.FromMail;
                                string toEmail = contactDetails.MailId;
                                string subject = "One Time Password";
                                string body = "Please use this OTP : " + myRandomNo + " to log into " + provider.Value + "'s" + " RPM portal,Do not share it with anyone";
                                SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587);
                                smtpClient.EnableSsl = true;
                                smtpClient.Credentials = new NetworkCredential(fromEmail, contactDetails.Password);
                                MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                                smtpClient.Send(mailMessage);
                                ValidMailId = true;

                            }
                            catch (Exception ex)
                            {
                                return StatusCode(503,"Failed to send otp to Email,Please contact Admin");
                            }
                        }
                        else if (resp.IsSmsSend)
                        {
                            try
                            {
                                if (accsid != null && authtoken != null && smssid != null)
                                {
                                    List<SystemConfigInfo> lstConfigs = DalCommon.GetSystemConfig(CONN_STRING, "Client", "User");
                                    SystemConfigInfo? Code = lstConfigs.Find(x => x.Name.Equals("CountryCode"));
                                    TwilioCommMgr commMgr = new TwilioCommMgr();
                                    SendSms sendSms = new SendSms();
                                    if (Code == null)
                                    {
                                        throw new Exception("Something went wrong!");
                                    }
                                    sendSms.PhoneNo = Code.Value + contactDetails.MobileNumber; ;
                                    phonenumber = contactDetails.MobileNumber; ;
                                    sendSms.Message = "Please use this OTP : " + myRandomNo + " to log into " + provider.Value + "'s" + " RPM portal,Do not share it with anyone";
                                    msgSent = commMgr.MessagingService(sendSms, accsid.Value, authtoken.Value, smssid.Value);
                                    validMobileNumber = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                return StatusCode(503, "Failed to send otp to Mobile,Please contact Admin");
                            }
                        }

                        //send otp  to patient mobile
                    }
                    if (response.tkn == null)
                    {
                        int RetryAdded = RpmDalFacade.UpdateRetryCount(verPass.UserName);

                        if (RetryAdded > 0)
                        {
                            if (MfaRetryCount == null)
                            {
                                throw new Exception("Something went wrong!");
                            }
                            if (RetryAdded == Convert.ToInt32(MfaRetryCount.Value))
                            {
                                return Forbid("The user is temporarily locked. Please contact Admin /try again after 5 Minutes.");
                            }
                            else
                            {
                                return Unauthorized("Invalid Login details, Please check the username/passowrd.");
                            }

                        }
                        else
                        {
                            return Unauthorized();
                        }



                    }
                    else
                    {
                        List<Roles> roles = new List<Roles>();
                        roles = RpmDalFacade.GetUserRoles(verPass.UserName);
                        loginResponseToken.Roles = roles;
                        if (MfaLimit == null)
                        {
                            throw new Exception("Something went wrong!");
                        }
                        loginResponseToken.MFA = true;
                        loginResponseToken.TimeLimit = Convert.ToInt32(MfaLimit.Value);
                        string input = contactDetails.MailId;
                        string pattern = @"(?<=[\w]{1})[\w\-._\+%]*(?=[\w]{1}@)";
                        string result = Regex.Replace(input, pattern, m => new string('*', m.Length));
                        if (resp == null)
                        {
                            throw new Exception("Something went wrong!");
                        }
                        if (resp.IsMailSend && resp.IsSmsSend)
                        {
                            loginResponseToken.Mobilenumber = "xxxxxxxx" + phonenumber.Substring(phonenumber.Length - 2);
                            loginResponseToken.MailId = result;
                            loginResponseToken.ValidMailId = ValidMailId;
                            loginResponseToken.ValidMobile = validMobileNumber;
                        }
                        else if (resp.IsMailSend)
                        {
                            loginResponseToken.MailId = result;
                            loginResponseToken.ValidMailId = ValidMailId;
                            loginResponseToken.ValidMobile = validMobileNumber;

                        }
                        else if (resp.IsSmsSend)
                        {
                            loginResponseToken.Mobilenumber = "xxxxxxxx" + phonenumber.Substring(phonenumber.Length - 2);
                            loginResponseToken.ValidMailId = ValidMailId;
                            loginResponseToken.ValidMobile = validMobileNumber;
                        }


                        if (loginResponseToken.tkn != null)
                        {
                            return Ok(loginResponseToken);
                        }
                        else
                        {
                            return Unauthorized(loginResponseToken);
                        }
                    }
                }

                else
                {

                    if (MFA_Enable == 0)

                    {
                        bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);

                        if (UserLocked)
                        {
                            return Forbid("The User is locked, Please contact your careteam.");
                        }
                    }

                    RpmDalFacade.ConnectionString = CONN_STRING;
                    var response = RpmDalFacade.Login(verPass);
                    response.MFA = false;
                    if (response.tkn == null)
                    {
                        int RetryAdded = RpmDalFacade.UpdateRetryCount(verPass.UserName);

                        if (RetryAdded > 0)
                        {
                            if (RetryAdded == Convert.ToInt32(MfaRetryCount.Value))
                            {
                                return Forbid( "The User is locked, Please contact your careteam.");
                            }
                            else
                            {
                                return Unauthorized("Invalid Login details, Please check the username/passowrd.");
                            }

                        }
                        return Unauthorized();
                    }
                    else
                    {
                        //update login details table to reset the retry count and lock mechanism
                        int ret = RpmDalFacade.UpdateLoginDetails(verPass.UserName);
                        if (ret != 0)
                        {
                            return Ok(response);
                        }
                        return StatusCode(500, "Db update failed");
                    }
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
    }
    
}
