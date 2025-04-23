using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace RpmCloud.Controllers
{
    [Route("api/device")]
    public class DeviceController : ControllerBase
    {
        public readonly string CONN_STRING;
        public DeviceController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("timezone")]
        [HttpGet]
        public IActionResult GetWorldTimeZone()
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

                    List<WorldTimeZone> tz = RpmDalFacade.GetWorldTimeZone();
                    if (!tz.Equals(null))
                    {
                        return Ok( tz);
                    }
                    return NotFound();
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

        [Route("account/create/iglucose")]
        [HttpPost]
        public IActionResult CreateDeviceAccount([FromBody] DeviceRegister devreg)
        {
            if (devreg is null)
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
                    RegisterDeviceResponse resp = RpmDalFacade.RegisterPatientDevice(devreg);
                    if (resp!=null)
                    {
                        return StatusCode(resp.HttpRetCode, resp);
                    }
                    return NotFound();
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

        [Route("validatedevice/iglucose/{deviceid}")]
        [HttpPost]
        public IActionResult ValidateDevice([FromQuery] string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid))
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
                    bool result = RpmDalFacade.ValidateDevice(deviceid);
                    return Ok( result);
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

        [Route("removedevice/iglucose")]
        [HttpPost]
        public IActionResult RemoveDevice([FromBody] DeviceRegister devreg)
        {
            if (devreg is null)
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
                    RegisterDeviceResponse resp = RpmDalFacade.RemoveDevice(devreg);
                    if (resp != null)
                    {
                        return StatusCode(resp.HttpRetCode, resp);
                    }
                    return NotFound();
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

        [Route("resetdevice")]
        [HttpPost]
        public IActionResult ResetDevice([FromBody] DeviceRegister devreg)
        {
            if (devreg is null)
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
                    RegisterDeviceResponse resp = RpmDalFacade.ResetDevice(devreg);
                    if (resp != null)
                    {
                        return StatusCode(resp.HttpRetCode, resp);
                    }
                    return NotFound();
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

        [Route("updatedevicestatus")]
        [HttpPost]
        public IActionResult UpdateDeviceStatus([FromBody] UpdateDeviceStatus devstat)
        {
            if (devstat is null)
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
                    devstat.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    bool resp = RpmDalFacade.UpdateDeviceStatus(devstat);
                    if (resp == true)
                    {
                        return Ok( "Device Status updated");
                    }
                    return BadRequest("Cannot update Device Status updated");
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
        
        [Route("isdeviceavailable")]
        [HttpPost]
        public IActionResult RemoveDevice([FromBody] DeviceValidate info)
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
                    bool resp = RpmDalFacade.IsDeviceAvailable(info);
                    if (resp == true)
                    {
                        return Ok( resp);
                    }
                        return NotFound(resp);                    
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

        [Route("isdevicemodelavailable")]
        [HttpPost]
        public IActionResult DeviceModelCheck([FromBody] DeviceValidate info)
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
                    //bool resp = RpmDalFacade.IsDeviceAvailable(info);
                    bool resp = RpmDalFacade.IsDeviceModelAvailable(info);
                    if (resp == true)
                    {
                        return Ok( resp);
                    }
                    return NotFound(resp);
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
        [Route("AddDevice")]
        [HttpPost]
        public IActionResult AddDevice([FromBody] AddDevice info)
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
                    int resp = RpmDalFacade.AddDevice(info);
                    if (resp >0)
                    {
                        return Ok( resp);
                    }
                    return NotFound(resp);
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
        [Route("account/forwardtelemetry")]
        [HttpPost]
        public IActionResult ForwardTelemetry([FromBody] DeviceTelemetry dev)
        {
            if (Request.Headers.ContainsKey("tesplab"))
            {
                string? apikey = Request.Headers["tesplab"].FirstOrDefault();
                if(apikey== "123456789")
                {
                    RpmDalFacade.ConnectionString = CONN_STRING;
                    string DeviceType = RpmDalFacade.IsValidlifesenseDevice(dev.deviceId);
                    if (DeviceType == null)
                    {
                        return Unauthorized("Unknown Device");
                    }
                    if (!RpmDalFacade.StagingTableInsert(dev, DeviceType))
                    {
                        return BadRequest("Unexcepted error");
                    }                   
                    return Ok( "Sucess");
                }                
            }
            return BadRequest("fail");
        }
        //InsertPatienVendorConnectivity
        [Route("addpatientvendorconnectivity")]
        [HttpPost]
        public IActionResult BodyTraceData([FromBody] AddPatientVendorConn dev)
        {
            RpmDalFacade.ConnectionString = CONN_STRING;           
            if (!RpmDalFacade.InsertPatienVendorConnectivity(dev))
            {
                return BadRequest("Unexcepted error");
            }
            return Ok( "Sucess");
        }
        //bodytracedataInsert
        [Route("bodytrace")]
        [HttpPost]
        public IActionResult BodyTraceData([FromBody] bodytracedata dev)
        {           
            RpmDalFacade.ConnectionString = CONN_STRING;
            //string output = JsonConvert.SerializeObject(dev.values);
            //bodytraceweight output1 = JsonConvert.DeserializeObject<bodytraceweight>(output);
            if (!RpmDalFacade.bodytracedataInsert(dev))
            {
                return BadRequest("Unexcepted error");
            }
            return Ok( "Sucess");
        }

        [Route("account/forwardstatus")]
        [HttpPost]
        public IActionResult ForwardStatus([FromBody] DeviceTelemetryStatus dev)
        {
            if (Request.Headers.ContainsKey("tesplab"))
            {
                string? apikey = Request.Headers["tesplab"].FirstOrDefault();
                if (apikey == "123456789")
                {

                    return Ok( "Sucess");
                }
            }
            return BadRequest("fail");
        }
        [Route("validatedevice/{deviceid}")]
        [HttpPost]
        public IActionResult ValidateDevices([FromQuery] string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid))
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
                    bool result = RpmDalFacade.ValidateDevice(deviceid);
                    return Ok( result);
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