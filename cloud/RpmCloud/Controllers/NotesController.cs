using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace RpmCloud.Controllers
{
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        public readonly string CONN_STRING;
        public NotesController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        public static string Blob_Conn_String = String.Empty;
        [Route("addnote")]
        [HttpPost]
        public IActionResult AddNote([FromBody] NoteInfo Info)
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
                    var id = RpmDalFacade.AddNotes(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = "Note Added" });
                    }
                    return BadRequest(new { message = "Could not save note" });
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
        [Route("addnotev1")]
        [HttpPost]
        public IActionResult AddNote([FromBody] NoteInfo_V1 Info)
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
                    var id = RpmDalFacade.AddNotesDetails_V1(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = "Note Added" });
                    }
                    return BadRequest(new { message = "Could not save note" });
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        [Route("updatenotev1")]
        [HttpPost]
        public IActionResult UpdateNote([FromBody] NoteInfo_V1 Info)
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
                    var id = RpmDalFacade.UpdateNotesDetails_V1(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(new { message = "Note Updated" });
                    }
                    return BadRequest(new { message = "Could not save note" });
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
                    return BadRequest(new { message = "Invalid session." });
                }
                return BadRequest(new { message = "Unexpected Error." });
            }
        }
        
        [Route("updatenote")]
        [HttpPost]
        public IActionResult UpdateNote([FromBody] NoteInfo Info)
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
                    if (RpmDalFacade.UpdateNotes(Info))
                    {
                        return Ok(new { message = "Note updated" });
                    }
                    return BadRequest(new { message = "Could not update note" });
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

        [Route("deletepatientnotes")]
        [HttpPost]
        public IActionResult DeletePatientNotes([FromQuery] int noteid)
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

                    bool d = RpmDalFacade.DeletePatientNotes(noteid);

                    if (d.Equals(true))
                    {
                        return Ok(new { message = d });
                    }
                    else
                    {
                        return NotFound(new { message = "Could not delete note" });
                    }


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

        [Route("CallLogByCareTeam")]
        [HttpPost]
        public IActionResult DownloadInvoice([FromBody] CallLogReport callLogReport)
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
                    string Uri = RpmDalFacade.CallLogByCareTeam(Blob_Conn_String, "invoice", callLogReport.StartDate, callLogReport.EndDate, callLogReport.Userid, UserName);
                    
                    if (Uri !=null )

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound(new { message = "Could not find Call log reports" });
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



        //NonEstablishedCall Report

        [Route("NonEstablishedCallReport")]
        [HttpPost]
        public IActionResult NonEstablishedCallReport([FromBody] nonEstablishedCallReport dates)
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
                    string Uri = RpmDalFacade.NonEstablishedCallReport(Blob_Conn_String, "invoice",dates.StartDate,dates.EndDate, dates.RoleId, UserName);

                    if (Uri != null)

                    {
                        return Ok(new { message = Uri });
                    }
                    return NotFound("Could not find Non Established Call log reports");
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
