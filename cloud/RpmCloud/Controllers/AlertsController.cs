using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/alerts")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        public readonly string CONN_STRING;
        public AlertsController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }

        [HttpGet("worklistgetalerts")]
        public IActionResult GetAlerts(int RoleId)
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

                    DataSet alert = RpmDalFacade.GetAlerts(RoleId, UserName);
                    if (alert != null)
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }

        [HttpGet("getteamalertsbyidandalerttype")]
        public IActionResult GetTeamAlerts(string AlertType, int CareTeamId, int RoleId)
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

                    DataSet alert = RpmDalFacade.GetTeamAlertsById(AlertType, CareTeamId, RoleId, UserName);
                    if (alert != null)
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

        [HttpGet("worklistgetalertbyid")]
        public IActionResult GetAlertById([FromQuery] int Id)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    string s = Request.Headers["Bearer"].First();
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

                    GetAlert alert = RpmDalFacade.GetAlertById(Id);
                    if (alert != null)
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }

        [HttpGet("getalertbyidpatient")]
        public IActionResult GetAlertByIdPatient([FromQuery] int Id)
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

                    GetAlert alert = RpmDalFacade.GetAlertByIdPatient(Id);
                    if (alert != null)
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

        [HttpPost("savealertresponse")]
        public IActionResult SaveAlertResponse([FromBody] TaskResponse Info)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
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
                    var id = RpmDalFacade.AlertResponse(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not save alert details" });
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

        [HttpPost("savealertresponsepatient")]
        public IActionResult SaveAlertResponseFromPatient([FromBody] TaskResponse Info)
        {
            try
            {
                if (Request.Headers.ContainsKey("Bearer"))
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    string? s = Request.Headers["Bearer"].FirstOrDefault();
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
                    var id = RpmDalFacade.AlertResponseFromPatient(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not save alert details" });
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
