using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api")]
    public class NotificationController : ControllerBase
    {
        public readonly string CONN_STRING;
        public NotificationController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("notification/readstatus")]
        [HttpPut]
        public IActionResult UpdateReadNotification([FromBody] NotificationStatusUpdate Info)
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
                    Info.ModifiedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    var id = RpmDalFacade.UpdateNotificationReadStatus(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = "Notification Status updated" });
                    }
                    return BadRequest("Could not save notification");
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

        [Route("notification/user")]
        [HttpGet]
        public IActionResult GetNotification(DateTime? StartDate=null, int Count=10)
        {
            try
            {
                if (!StartDate.HasValue) StartDate = DateTime.Now;
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

                    SystemNotificationByUser notifications = RpmDalFacade.GetSystemNotificationsByUser(UserName, StartDate,
                                                                                        Count, UserName);
                    if (!(notifications == null))
                    {
                        return Ok(JsonConvert.SerializeObject(notifications, Formatting.Indented));
                    }
                    return NotFound("Could not find notification details");
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
        [Route("notification/count")]
        [HttpGet]
        public IActionResult GetNotificationCount()
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

                    SystemNotificationCount notificationcount = RpmDalFacade.GetSystemNotificationCount(UserName);
                    if (!(notificationcount == null))
                    {
                        return Ok(notificationcount);
                    }
                    return NotFound("Could not find notification details");
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


        
        [Route("notification/deletenotifications")]
        [HttpPost]
        public IActionResult DeleteNotification([FromBody] int notificationId)
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

                    bool isdelete = RpmDalFacade.DeleteSystemNotificationsByUser(notificationId,UserName);
                    if (isdelete)
                    {
                        return Ok(isdelete);
                    }
                    return NotFound("Could not find notification details");
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

        [Route("notification/insertfirebasetoken")]
        [HttpPost]
        public IActionResult InsertFirebaseToken([FromQuery] string Token)
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

                    bool isinsert = RpmDalFacade.InsertFirebaseToken(UserName, s, Token);
                    if (isinsert)
                    {
                        return Ok(isinsert);
                    }
                    return NotFound("Could not Insert Token");
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


    }
}