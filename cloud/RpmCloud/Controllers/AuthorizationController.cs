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
        public IActionResult Login([FromBody] RPMLogin verPass)
        {
            try
            {
                bool otprequired = false;
                RpmDalFacade.ConnectionString = CONN_STRING;
                if (verPass.UserName == null)
                {
                    return Unauthorized(new { message = "Unauthorized user." });
                }
                bool isActive = RpmDalFacade.CheckUserActive(verPass.UserName);

                if(!isActive)
                {
                    return Unauthorized(new { message = "Unauthorized user." });;
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
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized user." }); 
                }
                SystemConfigInfo? Mfa = lstConf.Find(x => x.Name.Equals("MFA"));
                SystemConfigInfo? MfaLimit = lstConf.Find(x => x.Name.Equals("MFATimeOut"));
                SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                if(Mfa == null || MfaLimit == null || MfaRetryCount == null)
                {
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized user." });
                }
                int MFA_Enable = Convert.ToInt32(Mfa.Value);
                if (MFA_Enable!=0 && otprequired)
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);
                    if (UserLocked)
                    {
                        return StatusCode(
                            StatusCodes.Status403Forbidden,
                            new { message = "The user is temporarily locked. Please contact Admin or try again after 5 minutes." }
                        );
                    }
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = RpmDalFacade.GetPhoneNumberByUserName(verPass.UserName);
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        Console.WriteLine("Invalid System Configurations");
                        return BadRequest(new { message = "Unauthorized user." });
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
                            Console.WriteLine("Invalid System Configurations");
                            return BadRequest(new { message = "Unauthorized user." });
                        }
                        SystemConfigInfo? provider = providerName.Find(x => x.Name.Equals("Provider"));
                        if(resp== null)
                        {
                            return BadRequest(new { message = "Invalid User" });
                        }
                        if (resp.IsMailSend && resp.IsSmsSend)
                        {
                            if(provider== null)
                            {
                                Console.WriteLine("Invalid Provider");
                                return BadRequest(new { message = "Unauthorized user." });
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
                                        Console.WriteLine("Invalid Country Code");
                                        return BadRequest(new { message = "Unauthorized user." });
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
                                if (provider == null) {
                                    Console.WriteLine("Invalid Provider");
                                    return BadRequest(new { message = "Unauthorized user." });
                                }
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
                                return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send otp to Email,Please contact Admin." }
                                );
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
                                        Console.WriteLine("Invalid Country Code");
                                        return BadRequest(new { message = "Unauthorized user." });
                                    }
                                    if(provider == null)
                                    {
                                        Console.WriteLine("Invalid Provider");
                                        return BadRequest(new { message = "Unauthorized user." });
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
                                return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send OTP to mobile. Please contact Admin." }
                                );

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
                                return StatusCode(
                                   StatusCodes.Status403Forbidden,
                                   new { message = "The user is temporarily locked. Please contact Admin or try again after 5 minutes." }
                               );
                            }
                            else
                            {
                                return Unauthorized(new { message = "Invalid Login details, Please check the username/passowrd." });
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unauthorized user.");
                            return Unauthorized(new { message = "Unauthorized user." });
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
                            Console.WriteLine("Invalid User");
                            return BadRequest(new { message = "Unauthorized user." });
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
                            return StatusCode(
                                   StatusCodes.Status403Forbidden,
                                   new { message = "The User is locked, Please contact your careteam." }
                               );
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
                                return StatusCode(
                                    StatusCodes.Status403Forbidden,
                                    new { message = "The User is locked, Please contact your careteam." }
                                );
                            }
                            else
                            {
                                Console.WriteLine("Unauthorized");
                                return Unauthorized(new { message = "Invalid Login details, Please check the username/passowrd." });
                            }

                        }
                        return Unauthorized(new { message = "Unauthorized" });
                    }
                    else
                    {
                        //update login details table to reset the retry count and lock mechanism
                        int ret = RpmDalFacade.UpdateLoginDetails(verPass.UserName);
                        if(ret!=0)
                        {
                            return Ok(response);
                        }
                        return StatusCode(
                                StatusCodes.Status500InternalServerError,
                                new { message = "Internal Server Error." }
                            );
                    }
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Unauthorized" });
            }
                               
        }
        [Route("UserloginVerifiy")]
        [HttpPost]
        public IActionResult LoginWithOtp([FromBody] RPMWeb.Data.Common.Login verPass)
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
                        return Unauthorized(new { message = "Invalid session." });
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
                                Console.WriteLine("Invalid System Configurations");
                                return BadRequest(new { message = "Unauthorized" });
                            }
                            SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                            if (loginDetails.Match.ToLower()=="nomatch" || loginDetails.RetryCount>Convert.ToInt32(MfaRetryCount.Value))
                            {
                                if(loginDetails.RetryCount>=Convert.ToInt32(MfaRetryCount.Value))
                                {
                                    bool resp = RpmDalFacade.LockUser(verPass.UserName);
                                    if (resp)
                                    {
                                        return StatusCode(
                                              StatusCodes.Status403Forbidden,
                                              new { message = "The User is locked, Please contact your careteam." }
                                          );
                                    }
                                    else
                                    {
                                        return StatusCode(
                                              StatusCodes.Status403Forbidden,
                                              new { message = "The User is locked, Please contact your careteam." }
                                          );
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid OTP");
                                    return BadRequest(new { message = "Unauthorized" });
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
                                   
                                    return Unauthorized(new { message = "Unauthorized" });
                                }
                            }
                        }
                        else
                        {
                            return StatusCode(
                                StatusCodes.Status403Forbidden,
                                new { message = "The User is locked, Please contact your careteam." }
                            );
                        }
                        
                        
                    }
                    else
                    {
                        return Unauthorized(new { message = "Unauthorized" });
                    }
                }
                else
                {
                    return Unauthorized(new { message = "Unauthorized" });
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return Unauthorized(new { message = "Unauthorized" });
            }

        }

        [Route("userresetpasswordverifiy")]
        [HttpPost]
        public IActionResult ResetWithOtp([FromBody] ResetPassword verPass)
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
                        return Unauthorized(new { message = "Invalid session." });
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
                                Console.WriteLine("Invalid System Configurations");
                                return BadRequest(new { message = "Unauthorized" });
                            }
                            SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                            if (loginDetails.Match.ToLower()=="nomatch" || loginDetails.RetryCount>Convert.ToInt32(MfaRetryCount.Value))
                            {
                                if (loginDetails.RetryCount>=Convert.ToInt32(MfaRetryCount.Value))
                                {
                                    bool resp = RpmDalFacade.LockUser(verPass.Username);
                                    if (resp)
                                    {
                                        return StatusCode(
                                           StatusCodes.Status403Forbidden,
                                           new { message = "The user is temporarily locked. Please contact Admin or try again after 5 minutes." }
                                       );
                                    }
                                    else
                                    {
                                        return StatusCode(
                                            StatusCodes.Status403Forbidden,
                                            new { message = "The user is temporarily locked. Please contact Admin or try again after 5 minutes." }
                                        );
                                    }
                                }
                                else
                                {
                                    return Unauthorized(new { message = "Unauthorized" });
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
                                        return Ok(new { message = "Password changed successfully, please re-login to continue." });
                                    }
                                    else
                                    {
                                        return Ok(new { message = "Password changed successfully, old sessions are still alive and please re-login to continue." });
                                    }
                                }
                                else
                                {
                                    return StatusCode(
                                            StatusCodes.Status403Forbidden,
                                            new { message = "Failed to Reset Password, Please try again." }
                                        );
                                    
                                }
                            }
                        }
                        else
                        {
                            return StatusCode(
                                            StatusCodes.Status403Forbidden,
                                            new { message = "OTP entered is not valid." }
                                        );
                        }
                    }
                    else
                    {
                        return Unauthorized(new { message = "Unauthorized" });
                    }
                }
                else
                {
                    return Unauthorized(new { message = "Unauthorized" });
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Unauthorized" });
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
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { message = "The User is locked, Please contact your careteam." }
                    );
                }
                bool isActive = RpmDalFacade.CheckUserActive(username);

                if (!isActive)
                {
                    return NotFound(new { message = "The User is Inactive" });
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
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized" });
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
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized" });
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
                        Console.WriteLine("Invalid System Configurations");
                        return BadRequest(new { message = "Unauthorized" });
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
                            return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send otp to Email,Please contact Admin." }
                                );
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
                            return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send otp to Mobile,Please contact Admin." }
                                );
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
                return NotFound(new { message = "Invalid Login details, Please check the username." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Unauthorized" });
            }
        }
        [Route("UnlockUser")]
        [HttpPost]
        public IActionResult UnlockUser([FromBody] UnlockUser unlockUser)
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(tok);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(tok))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }

                    bool res = RpmDalFacade.UnlockUser(unlockUser.UserId,unlockUser.Patientid);
                    if (res)
                    {
                        return Ok(new { message = "User/Patient Unlocked" });
                    }
                    return NotFound(new { message = "Could not find  details" });
                }
                else
                {
                    return  Unauthorized(new { message = "Unauthorized" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Unauthorized" });
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    Logout logout = new Logout();
                    logout.JwtToken = s;
                    logout.createdBy = UserName;
                    if (RpmDalFacade.LogOut(logout))
                    {
                        return Ok(new { message = "Logout Sucess" });
                    }
                    return NotFound(new { message = "Invalid session" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Invalid session." });

            }
        }
        [Route("updatepassword")]
        [HttpPost]
        public IActionResult UpdatePassword([FromBody] Updatepassword updatepassword)
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
                    msg.Status = "Failed to Update Password";
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
                return BadRequest("Exception");
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }

                    List<OperationalMasterData> Info = RpmDalFacade.GetOperationalMasterData(UserName);
                    if (!(Info == null))
                    {
                        return Ok(Info);
                    }
                    return NotFound(new { message = "Could not find  details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest(new { message = "Invalid session." });;
                }
                return BadRequest(new { message = "Unexpected Error." });
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }

                    List<Roles> Info = RpmDalFacade.GetRolesMasterData(UserName);
                    if (!(Info == null))
                    {
                        return Ok(Info);
                    }
                    return NotFound(new { message = "Could not find  details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest(new { message = "Invalid session." });;
                }
                return BadRequest(new { message = "Unexpected Error." });
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    DataSet Info = RpmDalFacade.GetMasterDataForStatesAndCities(UserName);
                    if (!(Info == null))
                    {
                        return Ok(JsonConvert.SerializeObject(Info, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find  details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest(new { message = "Invalid session." });;
                }
                return BadRequest(new { message = "Unexpected Error." });
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    UserPermission up = RpmDalFacade.GetUserAccessRights(UserName, RoleId);
                    if (up == null)
                    {
                        return BadRequest(new { message = "Bad Request" });
                    }
                    return Ok(up);
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest(new { message = "Invalid session." });;
                }
                return BadRequest(new { message = "Unexpected Error." });
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
        //                return Unauthorized(new { message = "Invalid session." });
        //            }
        //            string UserName = RpmDalFacade.IsSessionValid(s);
        //            if (string.IsNullOrEmpty(UserName))
        //            {
        //                return Unauthorized(new { message = "Invalid session." });
        //            }
        //            if (!RpmDalFacade.ValidateTkn(s))
        //            {
        //                return Unauthorized(new { message = "Invalid session." });
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
        //            return Unauthorized(new { message = "Invalid session." });
        //        }
        //    }
        //    catch
        //    {
        //        return BadRequest(new { message = "Unexpected Error." });
        //    }
        //}
        [Route("Patientlogin")]
        [HttpPost]
        public IActionResult AppPatientLogin([FromBody] RPMWeb.Data.Common.RPMLogin verPass)
        {
            try
            {
                bool otprequired = false;
                RpmDalFacade.ConnectionString = CONN_STRING;

                bool isActive = RpmDalFacade.CheckPatientActive(verPass.UserName);

                if (!isActive)
                {
                    return Unauthorized(new { message = "Unauthorized user." });;
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
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized" });
                }
                SystemConfigInfo? Mfa = lstConf.Find(x => x.Name.Equals("MFA"));
                SystemConfigInfo? MfaLimit = lstConf.Find(x => x.Name.Equals("MFATimeOut"));
                SystemConfigInfo? MfaRetryCount = lstConf.Find(x => x.Name.Equals("MFARetryCount"));
                if (Mfa == null)
                {
                    Console.WriteLine("Invalid System Configurations");
                    return BadRequest(new { message = "Unauthorized" });
                }
                int MFA_Enable = Convert.ToInt32(Mfa.Value);
                if (MFA_Enable != 0 && otprequired)
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    bool UserLocked = RpmDalFacade.CheckUserLockedStatus(verPass.UserName);
                    if (UserLocked)
                    {
                        
                        return StatusCode(
                            StatusCodes.Status403Forbidden,
                            new { message = "The user is temporarily locked. Please contact Admin /try again after 5 Minutes." }
                        );
                    }
                    ContactDetails contactDetails = new ContactDetails();
                    contactDetails = RpmDalFacade.GetPhoneNumberByUserName(verPass.UserName);
                    List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Twilio", "User");
                    if (lstConfig == null || lstConfig.Count == 0)
                    {
                        Console.WriteLine("Invalid System Configurations");
                        return BadRequest(new { message = "Unauthorized" });
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
                            Console.WriteLine("Invalid System Configurations");
                            return BadRequest(new { message = "Unauthorized" });
                        }
                        SystemConfigInfo? provider = providerName.Find(x => x.Name.Equals("Provider"));
                        if (resp == null || provider == null)
                        {
                            Console.WriteLine("Invalid System Configurations");
                            return BadRequest(new { message = "Unauthorized" });
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
                                return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send otp to Mobile,Please contact Admin." });

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
                                        Console.WriteLine("Invalid System Configurations");
                                        return BadRequest(new { message = "Unauthorized" });
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
                                return StatusCode(
                                    StatusCodes.Status503ServiceUnavailable,
                                    new { message = "Failed to send otp to Mobile,Please contact Admin." }
                                );
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
                                Console.WriteLine("Invalid System Configurations");
                                return BadRequest(new { message = "Unauthorized" });
                            }
                            if (RetryAdded == Convert.ToInt32(MfaRetryCount.Value))
                            {
                                return StatusCode(
                                    StatusCodes.Status403Forbidden,
                                    new { message = "The user is temporarily locked. Please contact Admin /try again after 5 Minutes." }
                                );
                                
                            }
                            else
                            {
                                Console.WriteLine("Invalid Login details, Please check the username/passowrd.");
                                return BadRequest(new { message = "Unauthorized" });
                            }

                        }
                        else
                        {
                            return Unauthorized(new { message = "Unauthorized" });
                        }



                    }
                    else
                    {
                        List<Roles> roles = new List<Roles>();
                        roles = RpmDalFacade.GetUserRoles(verPass.UserName);
                        loginResponseToken.Roles = roles;
                        if (MfaLimit == null)
                        {
                            Console.WriteLine("Invalid System Configurations");
                            return BadRequest(new { message = "Unauthorized" });
                        }
                        loginResponseToken.MFA = true;
                        loginResponseToken.TimeLimit = Convert.ToInt32(MfaLimit.Value);
                        string input = contactDetails.MailId;
                        string pattern = @"(?<=[\w]{1})[\w\-._\+%]*(?=[\w]{1}@)";
                        string result = Regex.Replace(input, pattern, m => new string('*', m.Length));
                        if (resp == null)
                        {
                            Console.WriteLine("Invalid System Configurations");
                            return BadRequest(new { message = "Unauthorized" });
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
                            return StatusCode(
                                    StatusCodes.Status403Forbidden,
                                    new { message = "The User is locked, Please contact your careteam." }
                                );
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
                                return StatusCode(
                                    StatusCodes.Status403Forbidden,
                                    new { message = "The User is locked, Please contact your careteam." }
                                );
                            }
                            else
                            {
                                return Unauthorized(new { message = "Invalid Login details, Please check the username/passowrd." });
                            }

                        }
                        return Unauthorized(new { message = "Unauthorized" });
                    }
                    else
                    {
                        //update login details table to reset the retry count and lock mechanism
                        int ret = RpmDalFacade.UpdateLoginDetails(verPass.UserName);
                        if (ret != 0)
                        {
                            return Ok(response);
                        }
                        return StatusCode(500, new { message = "Db update failed" });
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Unauthorized" });
            }

        }
    }
    
}
