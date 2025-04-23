using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace RpmCloud.Controllers
{
    [Route("api/devices")]

    public class DeviceFeatureController : ControllerBase
    {
        public readonly string CONN_STRING;
        public DeviceFeatureController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }

        [Route("adddevice")]
        [HttpPost]
        public IActionResult AddDevice([FromBody] AddDevicePro info)
        {
            if (info is null)
            {
                return BadRequest("Invalid input data");
            }
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
                    ReturnMsg resp = RpmDalFacade.AddDeviceProc(info);
                    if (resp.Val == 1)
                    {
                        return Ok( resp.Msg);
                    }
                    return BadRequest(resp.Msg);
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
        [Route("updatedevice")]
        [HttpPost]
        public IActionResult UpdateDevice([FromBody] UpdateDevice info)
        {
            if (info is null)
            {
                return BadRequest("Invalid input data");
            }
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
                    int resp = RpmDalFacade.UpdateDeviceProc(info);
                    if (resp != 0)
                    {
                        return Ok( resp);
                    }
                    return BadRequest("Cannot Update Device");
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
        [Route("devicemasterdata")]
        [HttpGet]
        public IActionResult GetDevicesMastrData()
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

                    DataSet Device = RpmDalFacade.GetDeviceMasterData(UserName);
                    if (!(Device == null))
                    {
                        return Ok(JsonConvert.SerializeObject(Device, Formatting.Indented));
                    }
                    return NotFound("Could not find Device details");
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
        [Route("adddevicevendor")]
        [HttpPost]
        public IActionResult AddDeviceVendor([FromBody] AddDeviceVendor info)
        {
            if (info is null)
            {
                return BadRequest("Invalid input data");
            }
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
                    ReturnMsg resp = RpmDalFacade.AddDeviceVendor(info);
                    if (resp.Val == 1)
                    {
                        return Ok( resp.Msg);
                    }
                    else
                    {
                        return BadRequest(resp.Msg);
                    }
                    
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
        [Route("updatedevicevendor")]
        [HttpPost]
        public IActionResult UpdateDeviceVendor([FromBody] UpdateDeviceVendor info)
        {
            if (info is null)
            {
                return BadRequest("Invalid input data");
            }
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
                    int resp = RpmDalFacade.UpdateDeviceVendor(info);
                    if (resp != 0)
                    {
                        return Ok( resp);
                    }
                    else
                    {
                        return BadRequest("Cannot Update DeviceVendor Details");
                    }
                    
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
        [Route("getalldevices")]
        [HttpGet]
        public IActionResult GetAllDevices()
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
                    List<DeviceInfo> Device = RpmDalFacade.GetDeviceInfo(UserName);
                    if (!(Device == null))
                    {
                        return Ok( Device);
                    }
                    else
                    {
                        return NotFound("Could not find Device details");
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
                    return BadRequest(new { message = "Invalid session." });;
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("isvalidvendorcode/{Code}")]
        [HttpPost]
        public IActionResult IsValidVendorCode([FromQuery] string Code)
        {
            if (Code is null)
            {
                return BadRequest("Invalid input data");
            }
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
                    bool resp = RpmDalFacade.IsValidVendorCode(Code,UserName);
                    if (resp == true)
                    {
                        return Ok(new { message = resp });
                    }
                    else
                    {
                        return BadRequest(new { message = resp });
                    }
                    
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
