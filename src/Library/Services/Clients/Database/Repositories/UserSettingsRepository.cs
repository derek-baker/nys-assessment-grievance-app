using Library.Models;
using Library.Models.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using System.Linq;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public class UserSettingsRepository
    {
        private readonly IDocumentDatabase _db;
        private readonly IMongoCollection<BsonDocument> _collection;
        
        public UserSettingsRepository(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _collection = _db.GetCollection(dbSettings.UserSettingsCollectionName);
        }

        public async Task<UserSettings> GetUserSettings()
        {
            var documents = await _collection.FindAsync(doc => true);

            var reps = (await documents.ToListAsync())
                .Select(d => BsonSerializer.Deserialize<UserSettingsDocument>(d));

            return reps.FirstOrDefault()?.UserSettings;
        }

        public async Task SetUserSettings(UserSettings settings)
        {
            await _collection.DeleteManyAsync(doc => true);

            var document = new BsonDocument
            {
                { UserSettingsDocument.Fields.UserSettings, settings.ToBsonDocument() }
            };
            await _db.InsertDocument(_collection, document);
        }
    }
}
