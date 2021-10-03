using Library.Models.Entities;
using Library.Models.NoSQLDatabaseSchema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public class RepresentativesRepository
    {
        private readonly IDocumentDatabase _db;
        private readonly IMongoCollection<BsonDocument> _collection;
        
        public RepresentativesRepository(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _collection = _db.GetCollection(dbSettings.RepresentativesCollectionName);
        }

        public async Task<IEnumerable<RepGroupInfo>> GetRepresentatives()
        {
            var documents = await _collection.FindAsync(doc => true);

            var reps = (await documents.ToListAsync())
                .Select(d => BsonSerializer.Deserialize<RepGroupInfos>(d));

            return reps.FirstOrDefault()?.Representatives;
        }

        public async Task SetRepresentatives(IEnumerable<RepGroupInfo> reps)
        {
            await _collection.DeleteManyAsync(doc => true);

            var document = new BsonDocument
            {
                { RepresentativesDocument.Fields.Representatives, _db.BuildBsonArray(reps.ToList()) }
            };
            await _db.InsertDocument(_collection, document);
        }
    }
}
