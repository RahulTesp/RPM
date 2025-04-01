using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;


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
        public IActionResult AddDevice(AddDevicePro info)
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
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
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
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("updatedevice")]
        [HttpPost]
        public IActionResult UpdateDevice(UpdateDevice info)
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
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
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
                    return Unauthorized("Invalid session.");
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

                    DataSet Device = RpmDalFacade.GetDeviceMasterData(UserName);
                    if (!(Device == null))
                    {
                        return Ok( Device);
                    }
                    return NotFound("Could not find Device details");
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
        [Route("adddevicevendor")]
        [HttpPost]
        public IActionResult AddDeviceVendor(AddDeviceVendor info)
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
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
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
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("updatedevicevendor")]
        [HttpPost]
        public IActionResult UpdateDeviceVendor(UpdateDeviceVendor info)
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
                        return Unauthorized("Invalid session.");
                    }
                    string UserName = RpmDalFacade.IsSessionValid(s);
                    info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
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
                    return Unauthorized("Invalid session.");
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
        [Route("isvalidvendorcode/{Code}")]
        [HttpPost]
        public IActionResult IsValidVendorCode(string Code)
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
                    bool resp = RpmDalFacade.IsValidVendorCode(Code,UserName);
                    if (resp == true)
                    {
                        return Ok( resp);
                    }
                    else
                    {
                        return BadRequest(resp);
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
    }
}
