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
    public class UserRepository : IUserRepository
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
                        .Include(UserDocument.Fields.IsBuiltIn)
                        .Include(UserDocument.Fields.PasswordHash)
                        .Include(UserDocument.Fields.Salt)
                        .Include(UserDocument.Fields.HasNeverLoggedIn);

            var filter = Builders<BsonDocument>.Filter.Eq(UserDocument.Fields.UserName, username);

            var documents = await _db.GetDocuments(
                _collection,
                projection,
                filter);

            var userDoc = await documents.FirstOrDefaultAsync();
            return userDoc is null ? null : BsonSerializer.Deserialize<User>(userDoc);
        }

        public async Task RecordLogin(Guid userId)
        {
            await _db.UpdateDocumentField(
                _collection,
                idFieldName: UserDocument.Fields.UserId,
                documentId: userId.ToString(),
                fieldToUpdate: UserDocument.Fields.HasNeverLoggedIn,
                newFieldValue: false);
        }

        public async Task<User> GetUserById(Guid userId)
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
                projection,
                GetFilterForNonBuiltInUsers());

            var users = (await documents.ToListAsync())?.Select(u => BsonSerializer.Deserialize<User>(u));
            return users;
        }

        public async Task<string> CreateUser(string username)
        {
            var generatedPassword = GeneratePassword();
            var salt = HashService.GenerateSalt();
            var user = new User
            {
                UserId = Guid.NewGuid(),
                HasNeverLoggedIn = true,
                IsBuiltIn = false,
                UserName = username,
                Salt = HashService.ConvertSaltToString(salt),
                PasswordHash = HashService.HashData<string>(generatedPassword, salt)
            };
            var document = new BsonDocument
            {
                { UserDocument.Fields.UserId, user.UserId.ToString() },
                { UserDocument.Fields.HasNeverLoggedIn, user.HasNeverLoggedIn },
                { UserDocument.Fields.IsBuiltIn, user.IsBuiltIn },
                { UserDocument.Fields.UserName, user.UserName },
                { UserDocument.Fields.PasswordHash, user.PasswordHash },
                { UserDocument.Fields.Salt, user.Salt },
            };
            await _db.InsertDocument(_collection, document);
            return generatedPassword;
        }

        public async Task<string> CreateBuiltInUser(string username, string password)
        {
            var generatedPassword = GeneratePassword();
            var salt = HashService.GenerateSalt();
            var user = new User
            {
                UserId = Guid.NewGuid(),
                HasNeverLoggedIn = true,
                IsBuiltIn = true,
                UserName = username,
                Salt = HashService.ConvertSaltToString(salt),
                PasswordHash = HashService.HashData<string>(password, salt)
            };
            var document = new BsonDocument
            {
                { UserDocument.Fields.UserId, user.UserId.ToString() },
                { UserDocument.Fields.HasNeverLoggedIn, user.HasNeverLoggedIn },
                { UserDocument.Fields.IsBuiltIn, user.IsBuiltIn },
                { UserDocument.Fields.UserName, user.UserName },
                { UserDocument.Fields.PasswordHash, user.PasswordHash },
                { UserDocument.Fields.Salt, user.Salt },
            };
            await _db.InsertDocument(_collection, document);
            return generatedPassword;
        }

        public async Task<(string Password, User user)> ResetUserPassword(Guid userId)
        {
            var password = GeneratePassword();
            var salt = HashService.GenerateSalt();
            var hash = HashService.HashData<string>(password, salt);
            User user = null;

            var tasks = new List<Task>
            {
                _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: UserDocument.Fields.UserId,
                    documentId: userId.ToString(),
                    fieldToUpdate: UserDocument.Fields.PasswordHash,
                    newFieldValue: hash),

                _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: UserDocument.Fields.UserId,
                    documentId: userId.ToString(),
                    fieldToUpdate: UserDocument.Fields.Salt,
                    newFieldValue: HashService.ConvertSaltToString(salt)),

                Task.Run(async () => {
                    user = await GetUserById(userId);
                })
            };
            await Task.WhenAll(tasks);
            return (password, user);
        }

        public async Task DeleteUser(Guid userId)
        {
            var filter = GetFilterForNonBuiltInUsers() & GetFilterForUserDocument(userId);
            await _collection.DeleteOneAsync(filter);
        }

        private FilterDefinition<BsonDocument> GetFilterForNonBuiltInUsers() =>
            Builders<BsonDocument>.Filter.Ne(
                UserDocument.Fields.IsBuiltIn,
                true);

        private FilterDefinition<BsonDocument> GetFilterForUserDocument(System.Guid userId) =>
            Builders<BsonDocument>.Filter.Eq(
                UserDocument.Fields.UserId,
                userId.ToString());

        private FilterDefinition<BsonDocument> GetFilterUserById(System.Guid userId) =>
            Builders<BsonDocument>.Filter.Eq(UserDocument.Fields.UserId, userId.ToString());

        private string GeneratePassword() => PasswordService.Generate(20, 4);
    }
}
