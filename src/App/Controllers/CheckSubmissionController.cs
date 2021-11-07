using Library.Models;
using Library.Models.DataTransferObjects;
using Library.Models.DataTransferObjects.Output;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CheckSubmissionController : ControllerBase
    {
        private readonly GrievanceRepository _grievances;

        public CheckSubmissionController(GrievanceRepository grievances)
        {
            _grievances = grievances;
        }

        /// <summary>
        /// POST: api/CheckSubmission/PostCheckForPreviousSubmission
        /// </summary>        
        [HttpPost]
        [ActionName("PostCheckForPreviousSubmission")]
        public IActionResult PostCheckForPreviousSubmission(
            [FromBody] GrievanceProperties grievanceProps)
        {
            ConflictingSubmittersInfo conflicts = _grievances.GetConflictingSubmitters(grievanceProps.taxMapId);
            return Ok(new ResponseMessage() { Message = conflicts.EmailSubmitters });            
        }
    }
}
