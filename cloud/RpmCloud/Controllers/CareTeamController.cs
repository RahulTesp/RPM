using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/careteam")]
    public class CareTeamController : ControllerBase
    {
        public readonly string CONN_STRING;
        public CareTeamController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("addcareteam")]
        [HttpPost]
        public IActionResult AddCareTeam(CareTeams Info)
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
                    var id = RpmDalFacade.AddCareTeam(Info);
                    if (id>0)
                    {
                        return Ok(id);
                    }                   
                    return BadRequest("Could not save CareTeam details");
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
        
        [Route("updatecareteam")]
        [HttpPost]
        public IActionResult UpdateCareTeam(CareTeams Info)
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
                    int res = RpmDalFacade.UpdateCareTeam(Info);
                    bool ret = true;
                    if (res.Equals(0))
                    {
                        ret = false;
                    }
                    else if(res.Equals(3))
                    {
                        ret=false;
                    }
                    if (ret)
                    {
                        return Ok("CareTeam details updated");
                    }
                    else
                    {
                        if (res.Equals(3))
                        {
                            return BadRequest("Could not update CareTeam details,patients found Under this manager");
                        }
                        else
                        {
                            return BadRequest("Could not update CareTeam details");
                        }  
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

        [Route("getunassignedmembers")]
        [HttpGet]
        public IActionResult GetPatient()
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
                    List<NonAssignedCM> nonAssignedCMs = RpmDalFacade.GetNonAssignedCaerTeamMembers(UserName);
                    if (!(nonAssignedCMs == null))
                    {
                        return Ok(nonAssignedCMs);
                    }
                    return NotFound("Could not find clinic details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest("Invalid session.");
                }
                return BadRequest("Unexpected Error.");
            }
        }

        [Route("getteamtasks")]
        [HttpGet]
        public IActionResult GetTeamTasks( DateTime TodayDate, DateTime StartDate, DateTime EndDate, int RoleId)
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

                    DataSet teamTasks = RpmDalFacade.GetTeamTasks(TodayDate, StartDate, EndDate, RoleId, UserName);
                    if (!(teamTasks == null))
                    {
                        return Ok(teamTasks);
                    }
                    return NotFound("Could not find task details");
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

        [Route("getteamalerts")]
        [HttpGet]
        public IActionResult GetTeamAlerts(int RoleId)
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

                    DataSet teamAlerts = RpmDalFacade.GetTeamAlerts(RoleId, UserName);
                    if (!(teamAlerts == null))
                    {
                        //return Ok(teamAlerts);
                        return Ok(JsonConvert.SerializeObject(teamAlerts, Formatting.Indented));
                    }
                    return NotFound("Could not find task details");
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

        [Route("getteamalertbyid")]
        [HttpGet]
        public IActionResult GetTeamDetailedAlerts(int CareTeamId)
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

                    DataSet teamAlerts = RpmDalFacade.GetDetailedTeamAlerts(CareTeamId, UserName);
                    if (!(teamAlerts == null))
                    {
                        return Ok(teamAlerts);
                    }
                    return NotFound("Could not find task details");
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
                    return Unauthorized("Invalid session lifetime.");
                }
                return BadRequest("Unexpected Error.");
            }
        }

        [Route("teambyid")]
        [HttpGet]
        public IActionResult GetCareTeamById(int TeamId)
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

                    CareTeamInfo info = RpmDalFacade.GetTeamDetails(TeamId, UserName);
                    if (info != null)
                    {
                        return Ok(info);
                    }
                    return NotFound("Could not find task details");
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
                    return Unauthorized("Invalid session lifetime.");
                }
                return BadRequest("Unexpected Error.");
            }
        }

        [Route("team")]
        [HttpGet]
        public IActionResult GetCareTeam()
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

                    List <CareTeamBaseInfo> info = RpmDalFacade.GetTeamDetails(UserName);
                    if (info != null)
                    {
                        return Ok(info);
                    }
                    return NotFound("Could not find task details");
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
                    return Unauthorized("Invalid session lifetime.");
                }
                return BadRequest("Unexpected Error.");
            }
        }

        [Route("getpatientcareteammembers")]
        [HttpGet]
        public IActionResult GetPatientCareteamMembers()
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

                    List<PatientCareTeamMembers> info = RpmDalFacade.GetPatientCareteamMembers(UserName);
                    if (info != null)
                    {
                        return Ok(info);
                    }
                    return NotFound("Could not find task details");
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
                    return Unauthorized("Invalid session lifetime.");
                }
                return BadRequest("Unexpected Error.");
            }
        }
    }
}
