using Library.Models.Entities;
using Library.Models.NoSQLDatabaseSchema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public class SessionRepository
    {
        private readonly IDocumentDatabase _db;
        private readonly IMongoCollection<BsonDocument> _collection;
        
        public SessionRepository(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _collection = _db.GetCollection(dbSettings.SessionsCollectionName);
        }

        public async Task<Session> CreateSession(System.Guid userId)
        {
            var session = new Session
            {
                SessionId = System.Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddHours(8)
            };
            var document = new BsonDocument
            {
                { SessionDocument.Fields.SessionId, session.SessionId.ToString() },
                { SessionDocument.Fields.UserId, session.UserId.ToString() },
                { SessionDocument.Fields.CreatedAt, session.CreatedAt },
                { SessionDocument.Fields.ValidUntil, session.ValidUntil }
            };
            await _db.InsertDocument(_collection, document);
            return session;
        }

        public async Task<Session> GetUserSession(System.Guid userId, System.Guid sessionId)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                        .Include(SessionDocument.Fields.SessionId)
                        .Include(SessionDocument.Fields.ValidUntil);

            var filterUser = Builders<BsonDocument>.Filter
                .Eq(SessionDocument.Fields.UserId, userId.ToString());

            var filterSession = Builders<BsonDocument>.Filter
                .Eq(SessionDocument.Fields.SessionId, sessionId.ToString());

            //var sort = Builders<BsonDocument>.Sort.Descending(SessionDocument.Fields.CreatedAt);
            //var documents = await _db.GetSortedDocuments(
            //    _collection,
            //    projection,
            //    filter,
            //    sort);
            var documents = await _db.GetDocuments(
                _collection,
                projection,
                filterUser & filterSession);

            var sessionDoc = await documents.FirstOrDefaultAsync();
            return sessionDoc is null
                ? null
                : BsonSerializer.Deserialize<Session>(sessionDoc);
        }
    }
}
