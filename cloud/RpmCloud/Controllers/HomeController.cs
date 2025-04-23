using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/home")]
    public class HomeController : ControllerBase
    {
        public readonly string CONN_STRING;
        public HomeController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("getdashboardvitalcount")]
        [HttpGet]
        public IActionResult GetDashboardVitalCount([FromQuery] int Days,
                                                    [FromQuery] DateTime ToDate,
                                                    [FromQuery] int UtcOffset,
                                                    [FromQuery] int RoleId)
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

                    List<DashboardVitalsList> vitals = RpmDalFacade.GetDashboardVitalCount(Days, ToDate, UtcOffset, RoleId, UserName);
                    if (!vitals.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(vitals, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find vitals details" });
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
        [Route("getdashboardpatientstatus")]
        [HttpGet]
        public IActionResult GetDashboardPatientStatus([FromQuery] int RoleId)
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

                    List<DashboardPatientStatusList> status = RpmDalFacade.GetDashboardPatientStatus(RoleId, UserName);
                    if (!status.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(status, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find status details" });
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
        [Route("getdashboardalerts")]
        [HttpGet]
        public IActionResult GetDashboardAlerts([FromQuery] int RoleId)
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

                    List<DashboardAlerts> alert = RpmDalFacade.GetDashboardAlerts(RoleId, UserName);
                    if (!alert.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(alert, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find alert details" });
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
        [Route("Getdashboardtodaysalertsandtasks")]
        [HttpGet]
        public IActionResult GetDashboardTodaysAlertsandTasks([FromQuery] int RoleId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<DashboardAlertAndTask> alert = RpmDalFacade.GetDashboardTodaysAlertsandTasks(RoleId, StartDate, EndDate, UserName);
                    if (!alert.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(alert, Formatting.Indented));
                    }
                    return NotFound("Could not find alert details");
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
        [Route("Getdashboardteamoverview")]
        [HttpGet]
        public IActionResult GetDashboardTeamOverview([FromQuery] int RoleId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<DashboardTeamOverView> alert = RpmDalFacade.GetDashboardTeamOverview(RoleId, StartDate, EndDate, UserName);
                    if (!alert.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(alert, Formatting.Indented));
                    }
                    return NotFound("Could not find team details");
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
    }
}
