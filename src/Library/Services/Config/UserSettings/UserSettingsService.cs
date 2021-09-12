using Contracts;
using Library.Database;
using Library.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Library.Services.Config.UserSettings
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IDocumentDatabase _db;
        private readonly DocumentDatabaseSettings _dbSettings;

        public UserSettingsService(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _dbSettings = dbSettings;
        }

        public async Task<Models.Settings.UserSettings> GetUserSettings()
        {
            var settingsCollection = GetSettingsCollection();
            var settings = await _db.GetUserSettings(settingsCollection);
            return settings;
        }

        public async Task SetUserSettings(Models.Settings.UserSettings settings)
        {
            var settingsCollection = GetSettingsCollection();
            await _db.SetUserSettings(settingsCollection, settings);
        }

        private IMongoCollection<BsonDocument> GetSettingsCollection()
        {
            return _db.GetCollection(_dbSettings.UserSettingsCollectionName);
        }
    }
}
