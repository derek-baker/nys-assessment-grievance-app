using Library.Database;
using Library.Models;
using Library.Models.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RepresentativesController : ControllerBase
    {
        private readonly IDocumentDatabase _db;
        private readonly DocumentDatabaseSettings _dbSettings;
        private readonly IAuthService _auth;

        public RepresentativesController(
            IDocumentDatabase db, 
            IAuthService auth,
            DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _dbSettings = dbSettings;
            _auth = auth;
        }

        [HttpGet]
        [ActionName("GetAllReps")]
        public async Task<IActionResult> GetRepresentatives()
        {
            var repsCollection = _db.GetCollection(_dbSettings.RepresentativesCollectionName);
            var reps = await _db.GetRepresentatives(repsCollection);
            
            return Ok(reps);
        }

        [HttpPost]
        [ActionName("SetReps")]
        public async Task<IActionResult> SetRepresentatives([FromBody] SetRepsParams input)
        {
            var authResult = _auth.AuthenticateAndAuthorizeUser(input.userName, input.password);
            if (!authResult.IsAuthenticated)
                return StatusCode(StatusCodes.Status403Forbidden);            

            var repsCollection = _db.GetCollection(_dbSettings.RepresentativesCollectionName);
            await _db.SetRepresentatives(repsCollection, input.reps);
            return Ok();
        }
    }
}
