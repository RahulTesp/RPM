using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RpmCloud.Controllers
{
    [Route("api/patient")]
    public class PatientController : ControllerBase
    {
        public readonly string CONN_STRING;
        public PatientController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        public static string Blob_Conn_String = String.Empty;//"DefaultEndpointsProtocol=https;AccountName=clynxstorage;AccountKey=jeV61FXECjOd/lQo4teqTHKYf2u+O+5pD0UOJhkM6jCp39GWndwIFWQhq1evWr0nT5xTTlkjWwJL+ASt8xG3Hg==;EndpointSuffix=core.windows.net";
        public static string ContainerName = "rpmprofilepictures";
        public static string Doc_ContainerName = "rpmpatientdocuments";
        public static string Report_ContainerName = "rpmpatientreports";

        [Route("addpatient")]
        [HttpPost]
        public IActionResult RegisterPatient([FromBody] PatientDetails Info)
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
                    Info.UserName = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    NewPatientCredential newPatientCredential = RpmDalFacade.SavePatient(Info);
                    if (newPatientCredential == null)
                    {
                        return NotFound(new { message = "Could not save patient details" });
                    }

                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(newPatientCredential, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not save patient details" });
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

        [Route("addimage")]
        [HttpPost]
        public async Task<IActionResult> ImageUpload([FromBody] int PatientId)
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

                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }

                    var httpRequest = HttpContext.Request;
                    bool upload = false;
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        foreach (var file in httpRequest.Form.Files)
                        {
                            upload = RpmDalFacade.UploadProfilePicture(PatientId, file, file.FileName, Blob_Conn_String, ContainerName, UserName);
                        }
                    }
                    if (upload == true)
                    {
                        return Created();
                    }
                    else
                    {
                        return BadRequest(new { message = "Not a valid image file(support jpg and png) or size (100KB)" });
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
        [Route("adddocument")]
        [HttpPost]
        public IActionResult UploadDocument()
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

                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }

                    var httpRequest = HttpContext.Request;
                    bool upload = false;
                    UploadPatientDocument info = new UploadPatientDocument();
                    info.PatientId = Convert.ToInt32(httpRequest.Form["PatientId"]);
                    info.PatientProgramId = Convert.ToInt32(httpRequest.Form["PatientProgramId"]);
                    info.DocumentType = httpRequest.Form["DocumentType"];
                    info.DocumentName = httpRequest.Form["DocumentName"];
                    info.Blob_Conn_String = Blob_Conn_String;
                    info.ContainerName = Doc_ContainerName;
                    info.CreatedBy = UserName;
                    if (httpRequest.Form.Files.Count > 0)
                    {
                        foreach (var file in httpRequest.Form.Files)
                        {
                            var postedFile = file;
                            upload = RpmDalFacade.UploadPatientDocument(info, postedFile);
                        }
                    }
                    if (upload == true)
                    {
                        return Created();
                    }
                    else
                    {
                        return BadRequest(new { message = "Not a valid  file or size greater than 100KB" });
                    }
                }
                else
                {
                    return Unauthorized(new { message = "Invalid sessoin." });
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
        [Route("updatepatient")]
        [HttpPost]
        public IActionResult UpdatePatient([FromBody] PatientDetails Info)
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
                    Info.UserName = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (RpmDalFacade.UpdatePatient(Info))
                    {
                        //try
                        //{
                        //    if (Info.Status.ToLower() == "active" || Info.Status.ToLower() == "readytodischarge" || Info.Status.ToLower() == "discharged")
                        //    {
                        //        RpmDalFacade.NotifyPatientStatusChange(Info.Status.ToLower(), Info, UserName);
                        //    }
                        //}
                        //catch
                        //{
                        //    return Ok("Patient details updated");
                        //}
                        return Ok(new { message = "Patient details updated" });
                    }
                    return BadRequest(new { message = "Could not update patient details" });
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
        [Route("updatepatientpassword")]
        [HttpPost]
        public IActionResult UpdatePatientPassword([FromBody] ResetPatientPW info)
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
                    info.CreatedBy = UserName;

                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized(new { message = "Invalid session." });
                    }
                    NewPatientCredential newPatientCredential = RpmDalFacade.UpdatePatientPassword(info);
                    if (newPatientCredential == null)
                    {
                        return NotFound(new { message = "Could not update patient details" });
                    }

                    if (!newPatientCredential.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(newPatientCredential, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not update patient details" });
                    //if (RpmDalFacade.UpdatePatientPassword(info))
                    //{
                    //    return Ok("Patient details updated");
                    //}
                    //return BadRequest("Could not update patient details");
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
        [Route("getpatientlastbilledcyclebydate")]
        [HttpGet]
        public IActionResult GetPatientLastBilledCycleByDate([FromQuery] int patientId, [FromQuery] int patientProgramId, [FromQuery] DateTime billeddate)
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
                    List<LastBilleddata> LastBilleddata = RpmDalFacade.GetPatientLastBilledDetailsBydate(patientId, patientProgramId, billeddate);
                    if (LastBilleddata != null)

                    {
                        return Ok(JsonConvert.SerializeObject(LastBilleddata, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Route("getdraftpatients")]
        [HttpGet]
        public IActionResult DraftPatients()
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
                    List<DraftPatient> draftPatients = RpmDalFacade.GetDraftPatients(UserName);
                    if (!draftPatients.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(draftPatients, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getdraftpatientDetails")]
        [HttpGet]
        public IActionResult DraftPatientDetails([FromQuery] int PatientId)
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
                    PatientDetails draftPatientdetails = RpmDalFacade.GetDraftPatientDetails(PatientId, UserName);
                    if (!draftPatientdetails.PatientId.Equals(0))
                    {
                        return Ok(JsonConvert.SerializeObject(draftPatientdetails, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getPatientInfoForProgramInsert")]
        [HttpGet]
        public IActionResult GetPatientInfoForProgramInsert([FromQuery] int PatientId)
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
                    PatientInfoForProgramInsert patientInfoForProgramInsert = RpmDalFacade.GetPatientInfoForProgramInsert(PatientId, UserName);
                    if (!patientInfoForProgramInsert.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(patientInfoForProgramInsert, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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

        [Route("getpatient")]
        [HttpGet]
        public IActionResult GetPatient([FromQuery] int PatientId, [FromQuery] int PatientProgramId)
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

                    PatientNProgramDetails PatientList = RpmDalFacade.GetPatient(PatientId, PatientProgramId, UserName);
                    if (!(PatientList == null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientList, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getprogramdetailsmasterdataaddpatient")]
        [HttpGet]
        public IActionResult GetProgramDetailsMasterData(int RoleId)
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

                    ProgramDetailsMasterDataAddPatient programDetailsMaster = RpmDalFacade.GetProgramDetailsMasterData(RoleId, UserName);
                    if (!programDetailsMaster.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(programDetailsMaster, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Route("getavailabledevices")]
        [HttpGet]
        public IActionResult GetAvailableDevices(int VitalId)
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
                    List<DeviceDetails> devices = RpmDalFacade.GetDeviceDetails(VitalId, UserName);

                    if (!devices.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(devices, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find devices" });
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


        [Route("getallactivepatients")]
        [HttpGet]
        public IActionResult GetAllPatient([FromQuery] int Days, [FromQuery] string Vitals, [FromQuery] string PatientType, [FromQuery] int RoleId)
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

                    DataSet PatientList = RpmDalFacade.GetAllPatient(Days, Vitals, PatientType, RoleId, UserName);
                    if (!PatientList.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientList, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getallnonactivepatients")]
        [HttpGet]
        public IActionResult GetAllPatients([FromQuery] string PatientType, [FromQuery] string Vitals, [FromQuery] int RoleId)
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

                    List<GetAllPatientInfo> PatientList = RpmDalFacade.GetAllPatients(PatientType, Vitals, RoleId, UserName);
                    if (!PatientList.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientList, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Route("getallpatientslist")]
        [HttpGet]
        public IActionResult GetAllPatientsList([FromQuery] DateTime ToDate, [FromQuery] int UtcOffset, [FromQuery] int Days, [FromQuery] int RoleId)
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
                    List<GetAllPatientInfo> PatientList = RpmDalFacade.GetAllPatientsList(ToDate, UtcOffset, Days, RoleId, UserName);
                    if (!PatientList.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientList, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getallpatientsSmslist")]
        [HttpGet]
        public IActionResult GetAllPatientsSmsList([FromQuery] int RoleId)
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
                    List<GetAllPatientSmsInfo> PatientList = RpmDalFacade.GetAllPatientsSmsList(RoleId, UserName);
                    if (!PatientList.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientList, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getpatientlastpgmstatus")]
        [HttpGet]
        public IActionResult GetAllPatientsList([FromQuery] int PatientId, [FromQuery] int PatientProgramId)
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

                    string Status = RpmDalFacade.GetPatientLastPgmStatus(PatientId, PatientProgramId, UserName);
                    if (!Status.Equals(null))
                    {
                        return Ok(new { message = Status });
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("addpatientprogram")]
        [HttpPost]
        public IActionResult AddPatientProgram([FromBody] PatientProgramDetailsInsert Info)
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
                    var id = RpmDalFacade.SavePatientProgram(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not add patient program details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Route("addnewpatientprogram")]
        [HttpPost]
        public IActionResult AddNewPatientProgram([FromBody] PatientProgramDetailsInsert Info)
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
                    var id = RpmDalFacade.AddNewPatientProgram(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not add patient program details" });
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
        [Route("updatepatientprogramdetails")]
        [HttpPost]
        public IActionResult UpdateProgram([FromBody] UpdateProgramDetails Info)
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
                    var id = RpmDalFacade.UpdateProgram(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not update  program details" });
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
        [Route("updatepatientprogram")]
        [HttpPost]
        public IActionResult UpdatePatientProgram([FromBody] PatientProgramDetailsUpdate Info)
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
                    var id = RpmDalFacade.UpdatePatientProgram(Info);
                    if (id == true)
                    {
                        return Ok(new { message = "Patient program details updated" });
                    }
                    return BadRequest(new { message = "Could not update patient program details" });
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

        [Route("getallpatientprograms")]
        [HttpGet]
        public IActionResult GetAllPatientPrograms([FromQuery] int PatientId)
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
                    List<PatientAllPrograms> PatientPrograms = RpmDalFacade.GetAllPatientPrograms(PatientId, UserName);

                    if (!PatientPrograms.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(PatientPrograms, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient program details" });
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
        [Route("renewpatientprogram")]
        [HttpPost]
        public IActionResult RenewPatientProgram([FromBody] PatientProgramRenew Info)
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
                    var id = RpmDalFacade.RenewPatientProgram(Info);
                    if (id != 0)
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not renew patient program " });
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
        [Route("addpatientsymptoms")]
        [HttpPost]
        public IActionResult AddPatientSymptoms([FromBody] PatientSymptom Info)
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
                    var id = RpmDalFacade.AddPatientProgramSymptoms(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not add patient symptom details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Route("getsymptomsmasterdata")]
        [HttpGet]
        public IActionResult GetPatientsSymptoms()
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

                    List<Symptom> symptom = RpmDalFacade.GetSymptomsMasterData(UserName);
                    if (!symptom.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(symptom, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find symptom details" });
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
        [Route("updatepatientsymptoms")]
        [HttpPost]
        public IActionResult UpdatePatientSymptoms([FromBody] PatientSymptom Info)
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
                    if (RpmDalFacade.UpdatePatientProgramSymptoms(Info))
                    {
                        return Ok();
                    }
                    return BadRequest(new { message = "Could not update patient symptom details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Route("getpatientsymptoms")]
        [HttpGet]
        public IActionResult GetPatientSymptoms([FromQuery] int PatientId, [FromQuery] int PatientProgramId)
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

                    List<GetPatientSymptom> symptomList = RpmDalFacade.GetPatientSymptoms(PatientId, PatientProgramId, UserName);
                    if (!symptomList.Equals(null))
                    {
                        return Ok(symptomList);
                    }
                    return NotFound(new { message = "Could not find patient details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Route("addpatientmedication")]
        [HttpPost]
        public IActionResult AddPatientMedication([FromBody] PatientMedication Info)
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
                    var id = RpmDalFacade.AddPatientProgramMedication(Info);
                    if (!id.Equals(0))
                    {
                        // Insert Notification a noti
                        //RpmDalFacade.AddPatientNotification();
                        //MsgQueueWrapper.PushToQueue();
                        //MsgQueueWrapper.SendMessage();
                        return Ok(new { message = id });
                    }
                    return BadRequest(new { message = "Could not add patient symptom details" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Route("updatepatientmedication")]
        [HttpPost]
        public IActionResult UpdatePatientMedication([FromBody] PatientMedication Info)
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
                    if (RpmDalFacade.UpdatePatientProgramMedication(Info))
                    {
                        return Ok();
                    }
                    return BadRequest(new { message = "Could not update patient symptom details" });
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
        [Route("getpatientmedication")]
        [HttpGet]
        public IActionResult GetPatientMedication([FromQuery] int PatientId, [FromQuery] int PatientProgramId)
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

                    List<GetPatientMedication> List = RpmDalFacade.GetPatientMedication(PatientId, PatientProgramId, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
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
        [Route("getpatientcallnotes")]
        [HttpGet]
        public IActionResult GetPatientCallNotes([FromQuery] int PatientId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetNotes> List = RpmDalFacade.GetPatientCallNotes(PatientId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
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

        [Route("getmasterdatanotes")]
        [HttpGet]
        public IActionResult GetMasterDataNotes(int ProgramId, string Type)
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

                    NotesTypeMasterData GetMasterDataNotes = RpmDalFacade.GetMasterDataNotes(ProgramId, Type, UserName);
                    if (!GetMasterDataNotes.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(GetMasterDataNotes, Formatting.Indented));
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
        [Route("getpatientnotesbyprogram")]
        [HttpGet]
        public IActionResult GetPatientNotes([FromQuery] string ProgramName, [FromQuery] string Type, [FromQuery] int PatientNoteId)
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
                    GetPatientNotesQA GetPatientNotes = RpmDalFacade.GetPatientNotes(ProgramName, Type, PatientNoteId, UserName);
                    // List<NotesProgramMaster> GetMasterDataNotes = RpmDalFacade.GetMasterDataNotes(UserName);
                    if (!GetPatientNotes.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(GetPatientNotes, Formatting.Indented));
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

        [Route("getpatientcallnotesbyid")]
        [HttpGet]
        public IActionResult getpatientcallnotesbyid([FromQuery] int PatientNoteId)
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

                    NoteDetails Notes = RpmDalFacade.GetPatientCallNotesDetails(PatientNoteId, UserName);
                    if (Notes != null)
                    {
                        return Ok(JsonConvert.SerializeObject(Notes, Formatting.Indented));
                    }
                    return NotFound("Could not find patient notes details");
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
        [Route("getpatientreviewnotes")]
        [HttpGet]
        public IActionResult GetPatientReviewNotes([FromQuery] int PatientId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetNotes> List = RpmDalFacade.GetPatientReviewNotes(PatientId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        //List<GetAllNotes> GetPatientNotes(int PatientId, int PatientProgramId, string NoteType, DateTime StartDate, DateTime EndDate, string CreatedBy)
        [Route("getpatientnotes")]
        [HttpGet]
        public IActionResult GetPatientNotes([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] string NoteType, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetAllNotes> List = RpmDalFacade.GetPatientNotes(PatientId, PatientProgramId, NoteType, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
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
        [Route("getpatientreviewnotesbyid")]
        [HttpGet]
        public IActionResult getpatientreviewnotesbyid([FromQuery] int PatientNoteId)
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

                    NoteDetails note = RpmDalFacade.GetPatientReviewNotesDetails(PatientNoteId, UserName);
                    if (note != null)
                    {
                        return Ok(JsonConvert.SerializeObject(note, Formatting.Indented));
                    }
                    return NotFound("Could not find patient note details");
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
        [Route("getpatientvitalreadings")]
        [HttpGet]
        public IActionResult GetPatientVitalReadings([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    VitalReadings List = RpmDalFacade.GetPatientVitalReadings(PatientId, PatientProgramId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
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
        [Route("getpatientvitalreadingsv1")]
        [HttpGet]
        public IActionResult GetPatientVitalReadingswithDateTime([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    PatientVitalReadings List = RpmDalFacade.GetPatientVitalReadingswithDateTime(PatientId, PatientProgramId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
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

        [Route("getpatienthealthtrends")]
        [HttpGet]
        public IActionResult GetPatientHealthTrends([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<HealthTrends> List = RpmDalFacade.GetPatientHealthTrends(UserName,PatientId, PatientProgramId, StartDate, EndDate, UserName);

                    if (!(List == null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }

                    HealthTrends healthTrends = new HealthTrends();
                    healthTrends.Time = new List<DateTime>();
                    healthTrends.Values = new List<Values>();
                    return NotFound(JsonConvert.SerializeObject(healthTrends, Formatting.Indented));
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
        [Route("getpatientschedules")]
        [HttpGet]
        public IActionResult GetPatientSchedule([FromQuery] int PatientId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetSchedules> List = RpmDalFacade.GetPatientSchedule(PatientId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient schedule details" });
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
        [Route("getpatientAlertandtask")]
        [HttpGet]
        public IActionResult GetPatientAlertAndTask([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetAlertsAndTasks> List = RpmDalFacade.GetPatientAlertAndTask(PatientId, PatientProgramId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient Alerts And Tasks details" });
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
        [Route("getpatientcriticalalerts")]
        [HttpGet]
        public IActionResult GetPatientAlerts([FromQuery] int PatientId)
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

                    List<DashboardAlerts> alert = RpmDalFacade.GetPatientCriticalAlerts(PatientId, UserName);
                    if (!alert.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(alert, Formatting.Indented));
                    }
                    return NotFound("Could not find alert details");
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
        [Route("getpatientinteractiontime")]
        [HttpGet]
        public IActionResult GetPatientInteractionTime([FromQuery] int PatientId)
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

                    int time = RpmDalFacade.GetPatientInteractionTime(PatientId, UserName);
                    if (!time.Equals(null))
                    {
                        return Ok(new { message = time });
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
        [Route("searchpatient")]
        [HttpGet]
        public IActionResult SearchPatient([FromQuery] string PatientNumber)
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

                    SearchPatient search = RpmDalFacade.SearchPatient(PatientNumber, UserName);
                    if (!search.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(search, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getPatientVitalSummary")]
        [HttpGet]
        public IActionResult getPatientVitalSummary([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    List<PatientSummary> patientSummary = RpmDalFacade.GetPatientVitalSummary(UserName, StartDate, EndDate);
                    if (patientSummary != null)
                    {
                        return Ok(JsonConvert.SerializeObject(patientSummary, Formatting.Indented));
                    }
                    List<PatientSummary> patientSummarydata = new List<PatientSummary>();

                    return Ok(JsonConvert.SerializeObject(patientSummarydata, Formatting.Indented));
                    //return NotFound("Could not find patient details");
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
        [Route("getVitalSummaryDetails")]
        [HttpGet]
        public IActionResult getVitalSummaryDetails([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
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
                    VitalSummary patientSummary = RpmDalFacade.GetVitalSummaryDetails(UserName, startDate, endDate);
                    if (patientSummary != null)

                    {
                        return Ok(patientSummary);
                    }
                    VitalSummary patientSummarydata = new VitalSummary();
                    patientSummarydata.vitals = new List<Vitalslist>();
                    return Ok(JsonConvert.SerializeObject(patientSummarydata, Formatting.Indented));
                    // return NotFound("Could not find patient details");
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
        [Route("getpatientprofile")]
        [HttpGet]
        public IActionResult GetPatientInfo()
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

                    List<GetPatientInfo> ListId = RpmDalFacade.GetPatientInfo(UserName);
                    if (!ListId.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(ListId, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getpatientuploads")]
        [HttpGet]
        public IActionResult GetPatientUploads(int PatientId)
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
                    List<PatientDocuments> ListId = RpmDalFacade.PatientDocuments(PatientId, UserName);
                    if (!ListId.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(ListId, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient uploads" });

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
        [Route("getpatientbillingreport")]
        [HttpGet]
        public IActionResult getPatientBillingReport([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate, [FromQuery] string Clinic, [FromQuery] string CPTCode, [FromQuery] int? patientId, [FromQuery] int isMonth, [FromQuery] string Format)
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
                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }
                    string Uri = RpmDalFacade.GetPatientBillingReport(StartDate, EndDate, patientId, Clinic, CPTCode, isMonth, UserName, Format, Blob_Conn_String, Report_ContainerName);
                    if (Uri != "")

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound(new { message = "Could not find patient bill reports" });
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

        [Route("getpatientbillinginfo")]
        [HttpGet]
        public IActionResult getPatientBillingInfo([FromQuery] int isPast, [FromQuery] int isFuture, [FromQuery] int isToday, [FromQuery] int isCurrentMonth, [FromQuery] int isLastMonth)
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
                    List<BillingInfo> PatientBillReport = RpmDalFacade.GetPatientBillingInfo(isPast, isFuture, isToday, isCurrentMonth, isLastMonth, UserName);
                    if (PatientBillReport != null && PatientBillReport.Count > 0)

                    {
                        return Ok(JsonConvert.SerializeObject(PatientBillReport, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing info" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Unexpected Error. " + ex);
            }
        }
        [Route("getpatientbillinginfobyId")]
        [HttpGet]
        public IActionResult getPatientBillingData([FromQuery] int patientId)
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
                    List<PatientBilldata> PatientBilldata = RpmDalFacade.GetPatientBillingData(patientId, UserName);
                    if (PatientBilldata != null && PatientBilldata.Count > 0)

                    {
                        return Ok(JsonConvert.SerializeObject(PatientBilldata, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing info" });
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
        [Route("getRecentPatientVitalSummary/{dayCount:int=7}")]
        [HttpGet]
        public IActionResult getRecentPatientVitalSummary([FromQuery] int dayCount)
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
                    VitalSummaryMeasures patientSummary = RpmDalFacade.GetRecentPatientVitalSummary(UserName, dayCount);
                    if (patientSummary != null)

                    {
                        return Ok(JsonConvert.SerializeObject(patientSummary, Formatting.Indented));
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
        [Route("getbillinginfobyPatientId")]
        [HttpGet]
        public IActionResult getbillinginfobyPatientId([FromQuery] int patientId, [FromQuery] int patientProgramId)
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
                    List<PatientBilldata> PatientBilldata = RpmDalFacade.GetBillingDataByPatientId(patientId, patientProgramId, UserName);
                    if (PatientBilldata != null && PatientBilldata.Count > 0)

                    {
                        return Ok(JsonConvert.SerializeObject(PatientBilldata, Formatting.Indented));
                    }
                    //return NotFound("Could not find billing info");
                    return Ok(JsonConvert.SerializeObject(PatientBilldata, Formatting.Indented));
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("getsearchpatientlist")]
        [HttpGet]
        public IActionResult GetRecentPatientVitalSummary([FromQuery] int RoleId)
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
                    List<searchPatient> patientSummary = RpmDalFacade.SearchPatient(RoleId, UserName);
                    if (patientSummary != null)

                    {
                        return Ok(JsonConvert.SerializeObject(patientSummary, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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


        [Route("getpatientbillingDataList")]
        [HttpGet]
        public IActionResult getpatientbillingDataList([FromQuery] string patientType, [FromQuery] string patientFilter, [FromQuery] string patientId, [FromQuery] string patientName, [FromQuery] string program, [FromQuery] string assignedmember, [FromQuery] int Index, [FromQuery] string readingFilter, [FromQuery] string interactionFilter, [FromQuery] int RoleId, [FromQuery] string ProgramType)
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
                    PatientBilldataList PatientBilldata = RpmDalFacade.GetPatientBillingDataList(patientType, patientFilter, patientId, patientName, program, assignedmember, Index, readingFilter, interactionFilter, RoleId, UserName, ProgramType);
                    if (PatientBilldata != null)

                    {
                        return Ok(JsonConvert.SerializeObject(PatientBilldata, Formatting.Indented));
                    }
                    else
                    {
                        object[] emptyArray = new object[0];
                        return Ok(emptyArray);
                    }
                    //object[] emptyArray = new object[0];

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



        [Route("getpatientlastbilledcycle")]
        [HttpGet]
        public IActionResult GetPatientLastBilledCycle([FromQuery] int patientId, [FromQuery] int patientProgramId, [FromQuery] string status)
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
                    List<LastBilleddata> LastBilleddata = RpmDalFacade.GetPatientLastBilledDetails(patientId, patientProgramId, status);
                    if (LastBilleddata != null)

                    {
                        return Ok(JsonConvert.SerializeObject(LastBilleddata, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient details" });
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
        [Route("getpatientbillinginfoCounts")]
        [HttpGet]
        public IActionResult getPatientBillingInfoCounts([FromQuery] string BillingCode, [FromQuery] string Cycle, [FromQuery] int RoleId)
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
                    List<BillingInfoCounts> PatientBillingCounts = RpmDalFacade.GetPatientBillingInfoCounts(BillingCode, Cycle, RoleId, UserName);
                    if (PatientBillingCounts != null && PatientBillingCounts.Count > 0)

                    {
                        return Ok(JsonConvert.SerializeObject(PatientBillingCounts, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing info" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }

        [Route("getbillingtype")]
        [HttpGet]
        public IActionResult getBillingType()
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
                    BillingType billingType = RpmDalFacade.GetBillingType();
                    if (billingType != null)

                    {
                        return Ok(JsonConvert.SerializeObject(billingType, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing Type" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("getbillingCodes")]
        [HttpGet]
        public IActionResult getBillingCodes()
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
                    List<string> billingType = RpmDalFacade.GetBillingCodes();
                    if (billingType != null)

                    {
                        return Ok(JsonConvert.SerializeObject(billingType, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing Codes" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("getAllprograms")]
        [HttpGet]
        public IActionResult getAllprograms()
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
                    List<Programs> programs = RpmDalFacade.GetAllPrograms();
                    if (programs != null)

                    {
                        return Ok(JsonConvert.SerializeObject(programs, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find programs" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("getdiagnosiscodebyvitalid")]
        [HttpPost]
        public IActionResult getDiagnosisCodeByVitalId([FromBody] vitalIdsList VitalIds)
        {
            try
            {
                int[] VitalId = null;
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
                    List<ProgramVitalDignostics> codes = RpmDalFacade.GetDiagnosisCodeByVitalId(VitalIds);
                    if (codes != null)

                    {
                        return Ok(JsonConvert.SerializeObject(codes, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find billing Codes" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid session." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected Error." });
            }
        }

        [Route("DownloadInvoice")]
        [HttpGet]
        public IActionResult DownloadInvoice()
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
                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }
                    string Uri = RpmDalFacade.DownloadInvoice(Blob_Conn_String, "invoice");
                    if (Uri != "")

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound(new { message = "Could not find patient bill reports" });
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
        [Route("deletedraft")]
        [HttpPost]
        public IActionResult DeleteDraft([FromQuery] int PatientId)
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

                    bool d = RpmDalFacade.DeleteDraft(PatientId);

                    if (d.Equals(true))
                    {
                        return Ok(new { message = d });
                    }
                    else
                    {
                        return NotFound(new { message = "Could not delete patient" });
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

        [Route("deletepatientdocuments")]
        [HttpPost]
        public IActionResult DeletePatientDocuments([FromQuery] int documentid)
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

                    bool d = RpmDalFacade.DeletePatientDocuments(documentid);

                    if (d.Equals(true))
                    {
                        return Ok(new { message = d });
                    }
                    else
                    {
                        return NotFound(new { message = "Could not delete document" });
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

        [Route("getbillingreportdetails")]
        [HttpGet]
        public IActionResult getPatientBillingReportDetails([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate, [FromQuery] string Clinic, [FromQuery] int? patientId, [FromQuery] int isMonth, [FromQuery] string Format, string ProgramType)
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
                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }
                    string Uri = RpmDalFacade.GetPatientBillingReportDetails(StartDate, EndDate, patientId, Clinic, isMonth, UserName, Format, Blob_Conn_String, Report_ContainerName, ProgramType);
                    if (Uri != "")

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound(new { message = "Could not find patient bill reports" });
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
        [Route("getmissingbillingreportdetails")]
        [HttpGet]
        public IActionResult getPatientMissingBillingReportDetails([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate, [FromQuery] string Clinic, [FromQuery] int? patientId, [FromQuery] int isMonth, [FromQuery] string Format, string ProgramType)
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
                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }
                    string Uri = RpmDalFacade.GetPatientMissingBillingReportDetails(StartDate, EndDate, patientId, Clinic, isMonth, UserName, Format, Blob_Conn_String, Report_ContainerName, ProgramType);
                    if (Uri != "")

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound(new { message = "Could not find patient bill reports" });
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

        [Route("getpatientsmshistory")]
        [HttpGet]
        public IActionResult GetPatientSMSHistory([FromQuery] int PatientId, [FromQuery] int PatientProgramId, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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

                    List<GetSmsInfo> List = RpmDalFacade.GetPatientsSmsDetails(PatientId, PatientProgramId, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(JsonConvert.SerializeObject(List, Formatting.Indented));
                    }
                    return NotFound(new { message = "Could not find patient SMS details" });
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
