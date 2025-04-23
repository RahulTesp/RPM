using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/clinic")]
    public class ClinicController : ControllerBase
    {
        public readonly string CONN_STRING;
        public ClinicController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("addclinic")]
        [HttpPost]
        public IActionResult RegisterClinic([FromBody] ClinicInfo Info)
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
                    var id = RpmDalFacade.AddClinic(Info, UserName);
                    if (!id.Equals(0))
                    {
                        return Ok(id);
                    }
                    return BadRequest("Could not save clinic details");
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
        [Route("updateclinic")]
        [HttpPost]
        public IActionResult UpdateClinic([FromBody]UpdClinicInfo Info)
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
                    //Info.UserName = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (RpmDalFacade.UpdateClinic(Info,UserName))
                    {
                        return Ok(new { message = "clinic details updated" });
                    }
                    return BadRequest(new { message = "Could not update clinic details" });
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
        [Route("getclinic")]
        [HttpGet]
        public IActionResult GetPatient([FromQuery]int clinicid)
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

                    ClinicInfo clinicInfo = RpmDalFacade.GetClinic(clinicid);
                    if (!(clinicInfo == null))
                    {
                        return Ok(JsonConvert.SerializeObject(clinicInfo, Formatting.Indented));
                    }
                    return NotFound("Could not find clinic details");
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
        [Route("getallclinics")]
        [HttpGet]
        public IActionResult GetAllClinic()
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

                    List<ClinicAllInfo> clinicList = RpmDalFacade.GetAllClinic();
                    if (!clinicList.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(clinicList, Formatting.Indented));
                    }
                    return NotFound("Could not find patient details");
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
