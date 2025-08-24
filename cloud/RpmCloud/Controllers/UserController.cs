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
        public static string Blob_Conn_String = String.Empty;
        public static string ContainerName = "rpmprofilepictures";
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
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    string UserName1 = RpmDalFacade.IsSessionValid(s);
                    if (string.IsNullOrEmpty(UserName1))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }

                    GetUserProfiles userProfiles = RpmDalFacade.GetUserProfiles(UserId, UserName1);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(userProfiles, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
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

                    GetUserProfiles userProfiles = RpmDalFacade.GetMyProfiles(UserName);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(userProfiles, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
            }
        }
        [Route("adduser")]
        [HttpPost]
        public IActionResult RegisterUser([FromBody] UserProfiles Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    NewUserCredential newPatientCredential = RpmDalFacade.SaveUsers(Info);
                    if (newPatientCredential==null)
                    {
                        return NotFound(new { message = "Could not save patient details" });
                    }
                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(newPatientCredential, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not save patient details" });

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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("updateuser")]
        [HttpPost]
        public IActionResult UpdateUser([FromBody] UserProfiles Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (RpmDalFacade.UpdateUser(Info))
                    {
                        return Ok(new { message = "User details updated" });
                    }
                    return BadRequest(new { message = "Could not update user details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
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

                    DataSet users = RpmDalFacade.GetAllUsers(RoleId,UserName);
                    if (!users.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(users, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find users details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
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

                    ProileSummary userProfiles = RpmDalFacade.GetMyProfileAndProgram(UserName);
                    if (!userProfiles.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(userProfiles, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
            }
        }
        [Route("updateuserpassword")]
        [HttpPost]
        public IActionResult UpdateUserPassword([FromBody] ResetUserPW info)
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
                    info.CreatedBy = UserName;

                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (info.CreatedBy.ToLower() != "provider")
                    {
                        return NotFound(new { message = "Access denied! Only provider  is allowed to update user passwprd" });
                    }
                    NewPatientCredential newPatientCredential = RpmDalFacade.UpdateUserPassword(info);
                    if (newPatientCredential==null)
                    {
                        return NotFound(new { message = "Could not update user details" });
                    }

                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(newPatientCredential);
                    }
                    return NotFound(new { message = "Could not update user details" });
                    //if (RpmDalFacade.UpdatePatientPassword(info))
                    //{
                    //    return Ok("Patient details updated");
                    //}
                    //return BadRequest( "Could not update patient details");
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
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
                    string usernameexp =   RpmDalFacade.getUserName(userid);
                    if (UserName == usernameexp)
                    {
                        return Conflict(new { message = "Cannot deactivate user by self." });
                    }
                    DeactivateUser deactivateuser = RpmDalFacade.DeactivateUser(userid);
                    
                    if (deactivateuser.flag.Equals(1))

                    {
                        bool res1 = RpmDalFacade.ClearOldSessions(deactivateuser.username);
                        return Ok(new { message = deactivateuser.status });
                    }
                    else
                    {
                        return NotFound(deactivateuser.status);
                    }
                        
                              
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("UserStatusCheck")]
        [HttpPost]
        public IActionResult UserlockStatusCheck([FromBody] UnlockUser unlockUser)
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

                    bool res = RpmDalFacade.UserLockStatusCheck(unlockUser.UserId, unlockUser.Patientid);
                    unlockUser.isLocked=false;
                    if (res)
                    {
                        unlockUser.isLocked=true;
                        return Ok(JsonConvert.SerializeObject(unlockUser, Formatting.Indented));
                    }
                    return Ok(JsonConvert.SerializeObject(unlockUser, Formatting.Indented));
                }

                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception" });
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

                    string[] Languages = RpmDalFacade.GetLanguages();
                    if (!(Languages == null))
                    {
                        return Ok(JsonConvert.SerializeObject(Languages, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find Languages" });
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("addimage")]
        [HttpPost]
        public async Task<IActionResult> ImageUpload([FromQuery] int UserId)
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

                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }

                    var httpRequest = HttpContext.Request;
                    bool upload = false;
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        foreach (var file in httpRequest.Form.Files)
                        {
                            upload = RpmDalFacade.UploadUserProfilePicture(UserId, file, file.FileName, Blob_Conn_String, ContainerName, UserName);
                        }
                    }
                    if (upload == true)
                    {
                        //return Created(new { message = "Image Added to Patient" });
                        return Created(
                            uri: $"", // Location header
                            value: new { message = "Image Added to User" } // Response body
                        );
                    }
                    else
                    {
                        return BadRequest(new { message = "Not a valid image file(support jpg and png) or size (100KB)" });
                    }
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
                    return BadRequest(new { message = "Invalid session." }); ;
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }

    }

}

