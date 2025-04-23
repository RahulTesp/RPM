using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace RpmCloud.Controllers
{
    [Route("api/mob")]
    public class MobileController : ControllerBase
    {
        public readonly string CONN_STRING;
        public MobileController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("home")]
        [HttpGet]
        public IActionResult GetMobileHomeData([FromQuery] DateTime day, [FromQuery] int dayCount=7)
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

                    MobileHomeData data = RpmDalFacade.GetMobHomeData(UserName, day, dayCount);
                    if (data != null)
                    {
                        return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find notification details" });
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

    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        public readonly string CONN_STRING;
        public ConfigController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("client")]
        [HttpGet]
        public IActionResult GetClientConfig()
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

                    ClientConfig data = RpmDalFacade.GetClientConfig(UserName);
                    if (data != null)
                    {
                        return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find notification details" });
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
