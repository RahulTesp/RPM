using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace RpmCloud.Controllers
{
    [Route("api/patients")]
    public class WebAppPatientController : ControllerBase
    {
        public readonly string CONN_STRING;
        public WebAppPatientController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        public static string Blob_Conn_String = String.Empty;
        public static string Doc_ContainerName = "rpmpatientdocuments";
        [Route("getpatient")]
        [HttpGet]
        public IActionResult GetPatient()
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    PatientNProgramDetails PatientList = RpmDalFacade.GetPatient(data.PatientId, data.PatientProgramId, data.UserName);
                    ProileSummary userProfiles = RpmDalFacade.GetMyProfileAndProgram(data.UserName);
                    PatientList.ProfileSummary = userProfiles;
                    if (!(PatientList == null))
                    {
                        return Ok(PatientList);
                    }
                    return NotFound("Could not find patient details");
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
        [Route("gettodolist")]
        [HttpGet]
        public IActionResult GetToDoList([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    List<PatientToDoListResponse> todolist = RpmDalFacade.GetPatientToDoList(UserName, StartDate, EndDate);
                    if (todolist != null)
                    {
                        return Ok(todolist);
                    }
                    return NotFound("Could not find To do list details");

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

        [Route("getcurrentcyclereading")]
        [HttpGet]
        public IActionResult GetCurrentCycleReading()
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);

                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    PatientCurrentReading datas = RpmDalFacade.GetCurrentCycleReading(data.PatientId);
                    if (datas != null)
                    {
                        return Ok(datas);
                    }
                    return NotFound("Could not find Data");

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
        [Route("getpatientvitalreadings")]
        [HttpGet]
        public IActionResult GetPatientVitalReadings( [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    PatientVitalReadings List = RpmDalFacade.GetPatientVitalReadingswithDateTime(data.PatientId, data.PatientProgramId, StartDate, EndDate, data.UserName);

                    if (!List.Equals(null))
                    {
                        return Ok(List);
                    }
                    return NotFound("Could not find patient details");
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
        [Route("getpatienthealthtrends")]
        [HttpGet]
        public IActionResult GetPatientHealthTrends( [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    HealthTrends List = RpmDalFacade.GetPatientHealthTrends(data.UserName, data.PatientId, data.PatientProgramId, StartDate, EndDate, data.UserName);

                    if (!(List == null))
                    {
                        return Ok(List);
                    }
                    HealthTrends healthTrends = new HealthTrends();
                    healthTrends.Time = new List<DateTime>();
                    healthTrends.Values = new List<Values>();
                    return NotFound(healthTrends);
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
        [Route("getpatientmedication")]
        [HttpGet]
        public IActionResult GetPatientMedication()
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<GetPatientMedication> List = RpmDalFacade.GetPatientMedication(data.PatientId, data.PatientProgramId, data.UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(List);
                    }
                    return NotFound("Could not find patient details");
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
        [Route("getpatientsymptoms")]
        [HttpGet]
        public IActionResult GetPatientSymptoms()
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<GetPatientSymptom> symptomList = RpmDalFacade.GetPatientSymptoms(data.PatientId, data.PatientProgramId, data.UserName);
                    if (!symptomList.Equals(null))
                    {
                        return Ok(symptomList);
                    }
                    return NotFound("Could not find patient details");
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
        [Route("getpatientschedules")]
        [HttpGet]
        public IActionResult GetPatientSchedule( [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<GetSchedules> List = RpmDalFacade.GetPatientSchedule(data.PatientId, StartDate, EndDate, data.UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(List);
                    }
                    return NotFound("Could not find patient schedule details");
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
        [Route("getpatientuploads")]
        [HttpGet]
        public IActionResult GetPatientUploads()
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    List<PatientDocuments> ListId = RpmDalFacade.PatientDocuments(data.PatientId, data.UserName);
                    if (!ListId.Equals(null))
                    {
                        return Ok(ListId);
                    }
                    return NotFound("Could not find patient uploads");
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
                        return Unauthorized("Invalid session.");
                    }
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                    if (igc != null && igc.Count > 0)
                    {
                        SystemConfigInfo si = igc.Find(x => x.Name.Equals("ConnString"));
                        if (si == null) return NotFound("Storage string not found.");
                        Blob_Conn_String = si.Value;
                    }

                    var httpRequest = Request;
                    bool upload = false;
                    UploadPatientDocument info = new UploadPatientDocument 
                    {
                        PatientId = data.PatientId,
                        PatientProgramId = data.PatientProgramId,
                        DocumentType = httpRequest.Form["DocumentType"],
                        DocumentName = httpRequest.Form["DocumentName"],
                        Blob_Conn_String = Blob_Conn_String,
                        ContainerName = Doc_ContainerName,
                        CreatedBy = data.UserName
                    };
                    
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
                        return BadRequest("Not a valid image file(support jpg and png) or size (100KB)");
                    }
                }
                else
                {
                    return Unauthorized("Invalid sessoin.");
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
        [Route("addpatientmedication")]
        [HttpPost]
        public IActionResult AddPatientMedication(PatientMedication Info)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    Info.CreatedBy = data.UserName;
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    Info.PatientId = data.PatientId;
                    Info.PatientProgramId = data.PatientProgramId;
                    var id = RpmDalFacade.AddPatientProgramMedication(Info);
                    if (!id.Equals(0))
                    {
                        // Insert Notification a noti
                        //RpmDalFacade.AddPatientNotification();
                        //MsgQueueWrapper.PushToQueue();
                        //MsgQueueWrapper.SendMessage();
                        return Ok(id);
                    }
                    return BadRequest("Could not add patient medication.");
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
        [Route("updatepatientmedication")]
        [HttpPost]
        public IActionResult UpdatePatientMedication(PatientMedication Info)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    Info.CreatedBy = data.UserName;
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    Info.PatientId = data.PatientId;
                    Info.PatientProgramId = data.PatientProgramId;
                    if (RpmDalFacade.UpdatePatientProgramMedication(Info))
                    {
                        return Ok();
                    }
                    return BadRequest("Could not update patient medication details");
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

        [Route("getmynotes")]
        [HttpGet]
        public IActionResult GetPatientNotes( [FromQuery] string NoteType, [FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    List<GetAllNotes> List = RpmDalFacade.GetPatientNotes(data.PatientId, data.PatientProgramId, NoteType, StartDate, EndDate, UserName);
                    if (!List.Equals(null))
                    {
                        return Ok(List);
                    }
                    return NotFound("Could not find patient details");
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

        [Route("getmynotesbyid")]
        [HttpGet]
        public IActionResult GetPatientNotes(string ProgramName, string Type, int PatientNoteId)
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
                    GetPatientNotesQA GetPatientNotes = RpmDalFacade.GetPatientNotes(ProgramName, Type, PatientNoteId, UserName);
                    // List<NotesProgramMaster> GetMasterDataNotes = RpmDalFacade.GetMasterDataNotes(UserName);
                    if (!GetPatientNotes.Equals(null))
                    {
                        return Ok(GetPatientNotes);
                    }
                    return NotFound("Could not find patient details");
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

        [Route("getmydocumentbyid")]
        [HttpGet]
        public IActionResult getmydocumentbyid(int DocumentId)
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
                    PatientData data = RpmDalFacade.IsPatientSessionValid(s);
                    if (string.IsNullOrEmpty(data.UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }

                    PatientDocuments PatientDocument = RpmDalFacade.GetMyPatientDocuments(data.UserName, data.PatientId, data.PatientProgramId, DocumentId);

                    if (!(PatientDocument == null))
                    {
                        return Ok(PatientDocument);
                    }
                    
                    return NotFound(PatientDocument);
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
