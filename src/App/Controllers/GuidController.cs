using Library.Models;
using Library.Services.Clients.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GuidController : ControllerBase
    {
        private readonly GrievanceRepository _grievances;

        public GuidController(GrievanceRepository grievances)
        {
            _grievances = grievances;
        }

        /// <summary>
        /// Can be used to determine validity of a publicly-submitted guid.
        /// </summary>
        [HttpGet]
        [ActionName("TestGrievanceIdExistence")]
        public GuidTestResult TestGrievanceIdExistence(string guidString)
        {
            var isValid = _grievances.TestGuidExistence(guidString);
            
            BsonDocument doc = _grievances.GetByGuid(guidString);
            
            if (doc != null)
            {
                var grievance = BsonSerializer.Deserialize<GrievanceApplication>(doc);
                return new GuidTestResult(isValid, grievance.tax_map_id);
            }
            return new GuidTestResult(false);
        }
    }

    public class GuidTestResult
    {
        public bool IsValid { get; set; }
        public string TaxMapId { get; set; }

        public GuidTestResult(bool isValid, string taxMapId)
        {
            IsValid = isValid;
            TaxMapId = taxMapId;
        }

        public GuidTestResult(bool isValid)
        {
            IsValid = isValid;
        }
    }
}
