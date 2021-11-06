using Library.Models.Entities;
using Library.Models.NoSQLDatabaseSchema;
using Library.Services.Crypto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private FilterDefinition<BsonDocument> GetFilterUserById(System.Guid userId) => 
            Builders<BsonDocument>.Filter.Eq(UserDocument.Fields.UserId, userId.ToString());

        public async Task RecordLogin(System.Guid userId)
        {
            await _db.UpdateDocumentField(
                _collection,
                idFieldName: UserDocument.Fields.UserId,
                documentId: userId.ToString(),
                fieldToUpdate: UserDocument.Fields.HasNeverLoggedIn, 
                newFieldValue: false);
        }

        public async Task<User> GetUserById(System.Guid userId)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                        .Include(UserDocument.Fields.UserId)
                        .Include(UserDocument.Fields.UserName)
                        .Include(UserDocument.Fields.PasswordHash)
                        .Include(UserDocument.Fields.Salt)
                        .Include(UserDocument.Fields.HasNeverLoggedIn);

            var documents = await _db.GetDocuments(
                _collection,
                projection,
                GetFilterUserById(userId));

            var userDoc = await documents.FirstOrDefaultAsync();
            return userDoc is null
                ? null
                : BsonSerializer.Deserialize<User>(userDoc);
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                        .Include(UserDocument.Fields.UserId)
                        .Include(UserDocument.Fields.UserName)
                        .Include(UserDocument.Fields.HasNeverLoggedIn);

            var documents = await _db.GetDocuments(
                _collection,
                projection);

            var users = (await documents.ToListAsync())?.Select(u => BsonSerializer.Deserialize<User>(u));
            return users;
        }

        /// <summary>
        /// When creating normal users, password should be null.  
        /// </summary>
        public async Task<string> CreateUser(string username, string password = null)
        {
            var generatedPassword = PasswordService.Generate(26, 4);
            var salt = HashService.GenerateSalt();
            var user = new User
            {
                UserId = System.Guid.NewGuid(),
                HasNeverLoggedIn = true,
                UserName = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = HashService.HashData(password is null ? generatedPassword : password, salt)
            };
            var document = new BsonDocument
            {
                { UserDocument.Fields.UserId, user.UserId.ToString() },
                { UserDocument.Fields.HasNeverLoggedIn, user.HasNeverLoggedIn },
                { UserDocument.Fields.UserName, user.UserName },
                { UserDocument.Fields.PasswordHash, user.PasswordHash },
                { UserDocument.Fields.Salt, user.Salt },
            };
            await _db.InsertDocument(_collection, document);
            return generatedPassword;
        }

        public async Task DeleteUser(System.Guid userId)
        {
            var filterForNonBuiltInUsers = Builders<BsonDocument>.Filter
                .Ne(UserDocument.Fields.IsBuiltIn, true);

            var filterForDocToDelete = Builders<BsonDocument>.Filter.Eq(
                UserDocument.Fields.UserId, userId.ToString());

            await _collection.DeleteOneAsync(filterForNonBuiltInUsers & filterForDocToDelete);
        }
    }
}
