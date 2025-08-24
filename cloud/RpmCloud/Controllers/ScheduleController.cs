using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/schedules")]
    public class ScheduleController : ControllerBase
    {
        public readonly string CONN_STRING;
        public ScheduleController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("addschedule")]
        [HttpPost]
        public IActionResult AddSchedule([FromBody] ScheduleInfo Info)
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
                    var id = RpmDalFacade.AddSchedule(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not save Schedule details" });
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
                else if (ex.Message.Contains("Arithmetic overflow error"))
                {
                    return BadRequest(new { message = "Invalid range selection." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("updateschedule")]
        [HttpPost]
        public IActionResult UpdateSchedule([FromBody] ScheduleInfo Info)
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
                    if (RpmDalFacade.UpdateSchedule(Info))
                    {
                        return Ok(new { message = "Schedule details updated" });
                    }
                    return BadRequest(new { message = "Could not update Schedule details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Arithmetic overflow error"))
                {
                    return BadRequest(new { message = "Invalid range selection." });
                }
                else
                {
                    return BadRequest(new { message = "Exception" });
                }
            }
        }
        [Route("updatecurrentschedule")]
        [HttpPost]
        public IActionResult UpdateCurrentSchedule([FromBody] CurrentScheduleInfo Info)
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
                    if (RpmDalFacade.UpdateCurrentSchedule(Info))
                    {
                        return Ok(new { message = "Schedule details updated" });
                    }
                    return BadRequest(new { message = "Could not update Schedule details" });
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
      
        [Route("updatecompletedschedule")]
        [HttpPost]
        public IActionResult UpdateCompletedSchedule([FromBody] CompletedSchedules Info)
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
                    Info.ModifiedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (RpmDalFacade.UpdateScheduleCompletion(Info))
                    {
                        return Ok(new { message = "Schedule details updated" });
                    }
                    return BadRequest(new { message = "Could not update Schedule details" });
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
        [HttpGet("getschedulemasterdata")]
        public IActionResult GetMasterDataForSchedules([FromQuery] int RoleId)
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

                    DataSet details = RpmDalFacade.GetMasterDataForSchedules(RoleId, UserName);
                    if (!(details == null))
                    {
                        return Ok(JsonConvert.SerializeObject(details, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find schedule details" });
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
        [Route("getworklistschedules")]
        [HttpGet]
        public IActionResult GetWorklistSchedules(DateTime StartDate, DateTime EndDate)
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

                    List<GetSchedules> List = RpmDalFacade.GetCareTeamSchedule(StartDate,EndDate,UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find schedule details" });
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
        [Route("getworklistschedulesbyid")]
        [HttpGet]
        public IActionResult GetWorklistSchedulesBYId(int CurrentScheduleId)
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

                    ScheduleInfo schedule = RpmDalFacade.GetWorklistScheduleById( CurrentScheduleId, UserName);
                    if (!schedule.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(schedule, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find schedule details" });
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
        [Route("gettodolist")]
        [HttpGet]
        public IActionResult GetToDoList([FromQuery] DateTime day)
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
                    Schedule userProfiles = RpmDalFacade.GetToDoList(UserName, day);
                    if (userProfiles != null)
                    {
                        return Ok(JsonConvert.SerializeObject(userProfiles, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find Schedule details" });

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
    }
}
