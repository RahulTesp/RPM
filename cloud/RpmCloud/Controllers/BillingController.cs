using RPMWeb.Dal;
using Microsoft.AspNetCore.Mvc;

namespace RpmCloud.Controllers
{
    [Route("api/billing")]
    public class BillingController : ControllerBase
    {
        public readonly string CONN_STRING;
        public BillingController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }

        [Route("cptcode")]
        [HttpGet]
        public IActionResult PatientBillingInfo()
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
                    var id = 0;
                    if (id > 0)
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest("Could not save CareTeam details");
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
