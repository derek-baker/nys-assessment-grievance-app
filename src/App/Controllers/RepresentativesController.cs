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
        
        public RepresentativesController(RepresentativesRepository representatives)
        {
            _representatives = representatives;
        }

        [HttpGet]
        [ActionName("GetAllReps")]
        public async Task<IActionResult> GetRepresentatives()
        {
            var reps = await _representatives.GetRepresentatives();            
            return Ok(reps ?? new List<RepGroupInfo>());
        }

        [HttpPost]
        [CustomAuth]
        [ActionName("SetReps")]
        public async Task<IActionResult> SetRepresentatives([FromBody] SetRepsParams input)
        {
            await _representatives.SetRepresentatives(input.reps);
            return Ok();
        }
    }
}
