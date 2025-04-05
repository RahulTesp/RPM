using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        public readonly string CONN_STRING;
        public UserController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("getuserprofiles")]
        [HttpGet]
        public IActionResult Getuserprofiles(int UserId)
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
                    string UserName1 = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName1))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    GetUserProfiles userProfiles = RpmDalFacade.GetUserProfiles(UserId, UserName1);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(userProfiles);
                    }
                    return NotFound("Could not find patient details");
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

        [Route("getmyprofiles")]
        [HttpGet]
        public IActionResult GetMyprofiles()
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

                    GetUserProfiles userProfiles = RpmDalFacade.GetMyProfiles(UserName);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(userProfiles);
                    }
                    return NotFound("Could not find patient details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("adduser")]
        [HttpPost]
        public IActionResult RegisterUser(UserProfiles Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    NewUserCredential newPatientCredential = RpmDalFacade.SaveUsers(Info);
                    if (newPatientCredential==null)
                    {
                        return NotFound("Could not save patient details");
                    }
                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(newPatientCredential);
                    }
                    return NotFound("Could not save patient details");

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
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }
        [Route("updateuser")]
        [HttpPost]
        public IActionResult UpdateUser(UserProfiles Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (RpmDalFacade.UpdateUser(Info))
                    {
                        return Ok("User details updated");
                    }
                    return BadRequest( "Could not update user details");
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
        [Route("getallusers")]
        [HttpGet]
        public IActionResult GetAllusers(int RoleId)
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

                    DataSet users = RpmDalFacade.GetAllUsers(RoleId,UserName);
                    if (!users.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(users, Formatting.Indented));
                    }
                    return NotFound("Could not find users details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("getmyprofileandprogram")]
        [HttpGet]
        public IActionResult GetMyProfileAndProgram()
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

                    ProileSummary userProfiles = RpmDalFacade.GetMyProfileAndProgram(UserName);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(userProfiles);
                    }
                    return NotFound("Could not find patient details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("updateuserpassword")]
        [HttpPost]
        public IActionResult UpdateUserPassword(ResetUserPW info)
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
                    info.CreatedBy = UserName;

                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (info.CreatedBy.ToLower() != "provider")
                    {
                        return NotFound("Access denied! Only provider  is allowed to update user passwprd");
                    }
                    NewPatientCredential newPatientCredential = RpmDalFacade.UpdateUserPassword(info);
                    if (newPatientCredential==null)
                    {
                        return NotFound("Could not update user details");
                    }

                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(newPatientCredential);
                    }
                    return NotFound("Could not update user details");
                    //if (RpmDalFacade.UpdatePatientPassword(info))
                    //{
                    //    return Ok("Patient details updated");
                    //}
                    //return BadRequest( "Could not update patient details");
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
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }
        [Route("deactivateuser")]
        [HttpPost]
        public IActionResult DeactivateUser([FromQuery] int userid)
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
                    string usernameexp =   RpmDalFacade.getUserName(userid);
                    if (UserName == usernameexp)
                    {
                        return Conflict("Cannot deactivate user by self.");
                    }
                    DeactivateUser deactivateuser = RpmDalFacade.DeactivateUser(userid);
                    
                    if (deactivateuser.flag.Equals(1))

                    {
                        bool res1 = RpmDalFacade.ClearOldSessions(deactivateuser.username);
                        return Ok(deactivateuser.status);
                    }
                    else
                    {
                        return NotFound(deactivateuser.status);
                    }
                        
                              
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("UserStatusCheck")]
        [HttpPost]
        public IActionResult UserlockStatusCheck(UnlockUser unlockUser)
        {
            try
            {
                LoginDetails loginDetails = new LoginDetails();
                LoginResponseToken token = new LoginResponseToken();
                RpmDalFacade.ConnectionString = CONN_STRING;
                string tok = string.Empty;
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

                    bool res = RpmDalFacade.UserLockStatusCheck(unlockUser.UserId, unlockUser.Patientid);
                    unlockUser.isLocked=false;
                    if (res)
                    {
                        unlockUser.isLocked=true;
                        return Ok(unlockUser);
                    }
                    return Ok(unlockUser);
                }

                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }

        }



        [Route("getlanguages")]
        [HttpGet]
        public IActionResult GetLanguages()
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

                    string[] Languages = RpmDalFacade.GetLanguages();
                    if (!(Languages == null))
                    {
                        return Ok(Languages);
                    }
                    return NotFound("Could not find Languages");
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
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }

    }

}

