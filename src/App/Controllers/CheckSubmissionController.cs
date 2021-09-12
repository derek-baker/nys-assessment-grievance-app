using Library.Database;
using Library.Models;
using Library.Models.DataTransferObjects;
using Library.Models.DataTransferObjects.Output;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CheckSubmissionController : ControllerBase
    {
        private readonly IDocumentDatabase _dbClient;
        private readonly DocumentDatabaseSettings _dbSettings;

        public CheckSubmissionController(
            DocumentDatabaseSettings dbSettings,
            IDocumentDatabase dbClient)
        {
            _dbSettings = dbSettings;
            _dbClient = dbClient;
        }

        /// <summary>
        /// POST: api/CheckSubmission/PostCheckForPreviousSubmission
        /// </summary>        
        [HttpPost]
        [ActionName("PostCheckForPreviousSubmission")]
        [ProducesResponseType(typeof(ResponseMessage), StatusCodes.Status200OK)]
        public IActionResult PostCheckForPreviousSubmission(
            [FromBody] GrievanceProperties grievanceProps
        )
        {
            Contract.Requires(grievanceProps != null);
            ConflictingSubmittersInfo conflicts = _dbClient.GetConflictingSubmitters(
                _dbClient,
                grievanceProps.taxMapId,
                _dbSettings
            );
            return Ok(new ResponseMessage() { Message = conflicts.EmailSubmitters });            
        }
    }
}
