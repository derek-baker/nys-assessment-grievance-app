using Library.Models.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;
using Library.Models.Entities;
using Library.Services.Clients.Database.Repositories;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RepresentativesController : ControllerBase
    {
        private readonly RepresentativesRepository _representatives;
        private readonly IAuthService _auth;

        public RepresentativesController(
            IAuthService auth,
            RepresentativesRepository representatives)
        {
            _representatives = representatives;
            _auth = auth;
        }

        [HttpGet]
        [ActionName("GetAllReps")]
        public async Task<IActionResult> GetRepresentatives()
        {
            var reps = await _representatives.GetRepresentatives();
            
            return Ok(reps ?? new List<RepGroupInfo>());
        }

        [HttpPost]
        [ActionName("SetReps")]
        public async Task<IActionResult> SetRepresentatives([FromBody] SetRepsParams input)
        {
            var authResult = await _auth.AuthenticateAndAuthorizeUser(input.userName, input.password);
            if (!authResult.IsAuthenticated)
                return StatusCode(StatusCodes.Status403Forbidden);

            await _representatives.SetRepresentatives(input.reps);
            return Ok();
        }
    }
}
