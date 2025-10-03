using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;

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
                    return NotFound(new { message = "Not Found." });
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
                    return NotFound(new { message = "Not Found." });
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
                    return Ok(new { status = result });
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
                    return NotFound(new { message = "Not Found." });
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
                    return NotFound(new { message = "Not Found." });
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
                        return Ok(new { message = "Device Status updated" });
                    }
                    return BadRequest(new { message = "Cannot update Device Status updated" });
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
                        return Ok(new { response = resp });
                    }
                    return NotFound(new { response = resp });                    
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
                        return Ok(new { response = resp });
                    }
                    return NotFound(new { response = resp });
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
                        return Ok(new { response = resp });
                    }
                    return NotFound(new { response = resp });
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
        [Route("account/forwardtelemetry")]
        [HttpPost]
        public IActionResult ForwardTelemetry([FromBody] TranstekDeviceTelemetry dev)
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
                        return Unauthorized(new { message = "Unknown Device" });
                    }
                    if (!RpmDalFacade.StagingTableInsert(dev, DeviceType))
                    {
                        return BadRequest(new { message = "Unexcepted error" });
                    }                   
                    return Ok(new { message = "Sucess" });
                }                
            }
            return BadRequest(new { message = "fail" });
        }
        //InsertPatienVendorConnectivity
        [Route("addpatientvendorconnectivity")]
        [HttpPost]
        public IActionResult BodyTraceData([FromBody] AddPatientVendorConn dev)
        {
            RpmDalFacade.ConnectionString = CONN_STRING;           
            if (!RpmDalFacade.InsertPatienVendorConnectivity(dev))
            {
                return BadRequest(new { message = "Unexcepted error" });
            }
            return Ok(new { message = "Sucess" });
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
                return BadRequest(new { message = "Unexcepted error" });
            }
            return Ok(new { message = "Sucess" });
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

                    return Ok(new { message = "Sucess" });
                }
            }
            return BadRequest(new { message = "fail" });
        }
        [Route("validatedevice/{deviceid}")]
        [HttpPost]
        public IActionResult ValidateDevices([FromQuery] string deviceid)
        {
            if (string.IsNullOrEmpty(deviceid))
            {
                return BadRequest(new { message = "Invalid input data" });
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
                    return Ok( new { status = result });
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
        [Route("MakeDeviceAvailable")]
        [HttpPost]
        public IActionResult MakeDeviceAvailable([FromBody] deviceAvailable device)
        {
            if (device.DeviceNumber == string.Empty)
            {
                return BadRequest(new { message = "Invalid input data" });
            }
            try
            {
                HttpResponseMessage createRes = new HttpResponseMessage();
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
                    bool resp = RpmDalFacade.MakeDeviceAvailable(device.DeviceNumber);
                    if (resp)
                    {
                        return Ok(new { message = "The device has been made available to the Patients." });
                        
                    }
                    return NotFound(new { message = "Could not find device" });
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
		[Route("getDeviceType")]
        [HttpGet]
        public IActionResult GetDeviceType([FromQuery] string deviceModel)
        {
            try
            {
                HttpResponseMessage createRes = new HttpResponseMessage();
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

                    string DeviceType = RpmDalFacade.GetDeviceType(deviceModel);
                    if (!DeviceType.Equals(null))
                    {
                        return Ok(new { message = DeviceType });
                    }
                    return NotFound(new { message = DeviceType });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Exception." });
            }
        }
    }
}