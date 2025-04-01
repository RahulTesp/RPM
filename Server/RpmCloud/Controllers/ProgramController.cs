using RPMWeb.Dal;
using RPMWeb.Data.Common;
using Microsoft.AspNetCore.Mvc;

namespace RpmCloud.Controllers
{
    [Route("api/program")]
    public class ProgramController : ControllerBase
    {
        public readonly string CONN_STRING;
        public ProgramController(IConfiguration configuration)
        {
            CONN_STRING = configuration.GetSection("RPM:ConnectionString").Value ?? throw new ArgumentNullException(nameof(CONN_STRING));
        }
        [Route("getallprogramgoals")]
        [HttpGet]
        public IActionResult GetAllProgramGoals()
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

                    List<GetPgmGoals> ProgramGoalList = RpmDalFacade.GetAllPgmandGoals(UserName);
                    if (!ProgramGoalList.Equals(null))
                    {
                        return Ok(ProgramGoalList);
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
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }
        [Route("getprogramgoals")]
        [HttpGet]
        public IActionResult GetProgramGoals(int ProgramId)
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

                    List<GetPgmGoals> ProgramGoalList = RpmDalFacade.GetPgmandGoals(ProgramId,UserName);
                    if (!ProgramGoalList.Equals(null))
                    {
                        return Ok(ProgramGoalList);
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
                if (ex.Message.Contains("Lifetime validation failed"))
                {
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }
        [Route("addprogramgoals")]
        [HttpPost]
        public IActionResult AddProgramGoal(PgmGoals Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (RpmDalFacade.AddPgmandGoals(Info))
                    {
                        return Ok("Programgoal details saved");
                    }           
                    return BadRequest( "Could not save ProgramGoals details");
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
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }
        [Route("updateprogramgoals")]
        [HttpPost]
        public IActionResult Updateprogramgoals(PgmGoals Info)
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
                    Info.CreatedBy = UserName;
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (!RpmDalFacade.ValidateTkn(s))
                    {
                        return Unauthorized("Invalid session.");
                    }
                    if (RpmDalFacade.UpdatePgmandGoals(Info))
                    {
                        return Ok("Programgoal details updated");
                    }
                    return BadRequest( "Could not update programgoal details");
                }
                else
                {
                    return Unauthorized("Invalid session.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message);
            }
        }
        [Route("UpdateBillingPeriodsMedIT")]
        [HttpPost]
        public IActionResult RegisterPatient(BillingDatesUpdates Info)
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
                    var id = RpmDalFacade.UpdateBillDates(Info);
                    if (!id.Equals(0))
                    {
                        return Ok(id);
                    }
                    return BadRequest( "Could not save patient details");
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
                    return BadRequest( "Invalid session.");
                }
                return BadRequest( "Unexpected Error.");
            }
        }


    }
}
