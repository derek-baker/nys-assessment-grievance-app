using Library.Models.Entities;
using Library.Models.NoSQLDatabaseSchema;
using Library.Services.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public class UserRepository
    {
        private readonly IDocumentDatabase _db;
        private readonly IMongoCollection<BsonDocument> _collection;
        
        public UserRepository(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _collection = _db.GetCollection(dbSettings.UsersCollectionName);
        }

        public async Task CreateUser(string username, string password, bool forcePasswordReset)
        {
            var salt = HashService.GenerateSalt();
            var user = new User
            {
                UserId = System.Guid.NewGuid(),
                HasNeverLoggedIn = forcePasswordReset,
                UserName = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = HashService.HashData(password, salt)
            };
            var document = new BsonDocument
            {
                { UserDocument.Fields.UserId, user.UserId.ToString() },
                { UserDocument.Fields.HasNeverLoggedIn, user.HasNeverLoggedIn },
                { UserDocument.Fields.UserName, user.UserName },
                { UserDocument.Fields.PasswordHash, user.PasswordHash },
                { UserDocument.Fields.Salt, user.Salt },
            };
            await _db.InsertDocument(_collection, document);;
        }

        public async Task<User> GetUser(string username)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                        .Include(UserDocument.Fields.UserId)
                        .Include(UserDocument.Fields.UserName)
                        .Include(UserDocument.Fields.PasswordHash)
                        .Include(UserDocument.Fields.Salt)
                        .Include(UserDocument.Fields.HasNeverLoggedIn);

            var filter = Builders<BsonDocument>.Filter.Eq(UserDocument.Fields.UserName, username);

            var documents = await _db.GetDocuments(
                _collection,
                projection,
                filter);

            var userDoc = await documents.FirstOrDefaultAsync();
            return userDoc is null
                ? null
                : BsonSerializer.Deserialize<User>(userDoc);
        }
    }
}
