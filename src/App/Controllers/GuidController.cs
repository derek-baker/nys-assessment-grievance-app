using Library.Models;
using Library.Services.Clients.Database;
using Library.Services.Guid;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GuidController : ControllerBase
    {
        private readonly IDocumentDatabase _db;
        private readonly IGuidService _guid;
        private readonly DocumentDatabaseSettings _dbSettings;

        public GuidController(
            IDocumentDatabase db,
            IGuidService guidService,
            DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _guid = guidService;
            _dbSettings = dbSettings;
        }

        /// <summary>
        /// Can be used to determine GUID validity.
        /// </summary>
        [HttpGet("{guidString}")]
        [ActionName("TestGuidExistence")]
        public GuidTestResult TestGuidExistence(string guidString)
        {
            var isValid = _guid.TestGuidExistence(_db, _dbSettings, guidString);
            
            var collection = _db.GetCollection(_dbSettings.GrievancesCollectionName);

            // TODO: Refactor this to use more general method
            BsonDocument doc = _db.GetDocumentByGuid(collection, guidString);
            
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
