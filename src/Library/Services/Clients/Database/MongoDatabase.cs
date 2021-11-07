using Library.Models;
using Library.Models.NoSQLDatabaseSchema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Library.Services.Time;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database
{
    public class MongoDatabase : IDocumentDatabase
    {
        private readonly MongoClient _client;
        private readonly string _databaseName;

        public MongoDatabase(string connectionString, string databaseName)
        {
            _client = new MongoClient(connectionString);
            _databaseName = databaseName;
        }

        public static string BuildConnectionString(
            string dbUser,
            string password,
            string server,
            string dbName)
        {
            return $"mongodb+srv://{dbUser}:{password}@{server}/{dbName}?retryWrites=true&w=majority";
        }

        public IMongoCollection<BsonDocument> GetCollection(string collectionName)
        {
            var database = _client.GetDatabase(_databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        /// <param name="fieldsToInclude">list of fields to include in the projection</param>
        public ProjectionDefinition<BsonDocument> BuildProjection(ImmutableList<string> fieldsToInclude)
        {
            var projectionBuilder =
                Builders<BsonDocument>
                    .Projection;

            ProjectionDefinition<BsonDocument> projection = 
                projectionBuilder.Exclude(
                    GrievanceDocument.Fields.InternalDatabaseId
                );

            foreach (var field in fieldsToInclude)
            {
                projection = projection.Include(field);
            }
            return projection;
        }

        /// <summary>
        /// WARNING: Implementation assumes that the value used in 
        /// the filter occurs once in the collection.
        /// </summary>        
        public BsonDocument GetDocumentByStringField(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string fieldName,
            string fieldValue)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq(fieldName, fieldValue);

            var docs = collection.Find(filter).Project(projection);
            if (docs.CountDocuments() > 1) {
                throw new InvalidOperationException("Query result 'docs' has a count greater than 1. Only 1 and 0 are allowed.");
            }
            var document = docs.FirstOrDefault();
            
            return document;
        }

        /// <summary>
        /// Field used does not need to be unique.
        /// </summary>        
        public List<BsonDocument> GetDocumentsByStringField(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string fieldName,
            string fieldValue)
        {
            FilterDefinition<BsonDocument> filter =
                Builders<BsonDocument>.Filter.Eq(fieldName, fieldValue);

            IAsyncCursor<BsonDocument> cursor =
                collection.Find(filter).Project(projection).ToCursor();

            return cursor.ToList();
        }

        /// <summary>
        /// Field used does not need to be unique.
        /// </summary>   
        public List<BsonDocument> GetDocumentsByStringFieldCaseInsensitive(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string fieldName,
            string fieldValue)
        {
            FilterDefinition<BsonDocument> filter =
                Builders<BsonDocument>.Filter.Eq(fieldName, fieldValue);

            IAsyncCursor<BsonDocument> cursor = collection
                .Find(filter, new FindOptions { Collation = new Collation("en", strength: CollationStrength.Primary) })
                .Project(projection)
                .ToCursor();

            return cursor.ToList();
        }

        public Task<IAsyncCursor<BsonDocument>> GetDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection)
        {
            return collection
                .Find(doc => true)
                .Project(projection)
                .ToCursorAsync();
        }

        public Task<IAsyncCursor<BsonDocument>> GetDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            FilterDefinition<BsonDocument> filter)
        {
            return collection
                .Find(filter)
                .Project(projection)
                .ToCursorAsync();            
        }

        public Task<IAsyncCursor<BsonDocument>> GetSortedDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            FilterDefinition<BsonDocument> filter,
            SortDefinition<BsonDocument> sort)
        {
            return collection
                .Find(filter)
                .Project(projection)
                .Sort(sort)
                .ToCursorAsync();
        }

        public List<BsonDocument> GetDocumentsByTwoFields<T, U>(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string field1Name,
            T field1Value,
            string field2Name,
            U field2Value,
            bool field2ValueEquals = true
        )
        {
            var filter = Builders<BsonDocument>.Filter.Eq(field1Name, field1Value);
            var compositeFilter =
                (field2ValueEquals)
                    ? filter & Builders<BsonDocument>.Filter.Eq(field2Name, field2Value)
                    : filter & Builders<BsonDocument>.Filter.Ne(field2Name, field2Value);
            var documents = collection.Find(compositeFilter).Project(projection).ToCursor().ToList();
            return documents;
        }

        /// <summary>
        /// Does not apply a projection to the result of the query.
        /// </summary>   
        public BsonDocument GetDocumentByTwoStringFields(
            IMongoCollection<BsonDocument> collection,
            string field1Name,
            string field1Value,
            string field2Name,
            string field2Value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(field1Name, field1Value);
            var compositeFilter = filter & Builders<BsonDocument>.Filter.Eq(field2Name, field2Value);
            var document = collection.Find(compositeFilter).FirstOrDefault();
            return document;
        }

        public Task InsertDocument(
            IMongoCollection<BsonDocument> collection,
            BsonDocument document)
        {
            return collection.InsertOneAsync(document);
        }

        public async Task UpdateDocumentField<T>(
            IMongoCollection<BsonDocument> collection,
            string idFieldName,
            string documentId, 
            string fieldToUpdate, 
            T newFieldValue
        ) {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(idFieldName, documentId);

            var update =
                Builders<BsonDocument>
                    .Update.Set(fieldToUpdate, newFieldValue);

            await collection.UpdateOneAsync(filter, update);
        }

        public NysRps525OnlineFormAnswers GetNysRp525Answers(IMongoCollection<BsonDocument> collection, string submissionId)
        {
            var listOfFields = 
                typeof(GrievanceApplication).GetProperties()
                    .Select(p => p.Name).ToList();

            var projection = BuildProjection(listOfFields.ToImmutableList());

            var bsonDocument = GetDocumentByStringField(
                collection, 
                projection, 
                fieldName: GrievanceDocument.Fields.GuidString, 
                fieldValue: submissionId
            );
            var grievance = BsonSerializer.Deserialize<GrievanceApplication>(bsonDocument);

            NysRps525OnlineFormAnswers answers = null;
            if (grievance.nys_rp525_answers != null)
            {
                answers = JsonSerializer.Deserialize<NysRps525OnlineFormAnswers>(
                    grievance.nys_rp525_answers
                );
            }
            return answers;
        }

        public void UpdateNysRp525Answers(
            IMongoCollection<BsonDocument> collection,
            string submissionId,
            NysRps525OnlineFormAnswers answers,
            bool isComplete = false
        )
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, submissionId);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.NysRP525Answers, JsonSerializer.Serialize(answers))
                        .Set(GrievanceDocument.Fields.NysRP525Tentative, answers.Admin_Rp525_Tentative)
                        .Set(GrievanceDocument.Fields.NysRP525IsReduced, answers.Admin_Rp525_Check2a)
                        .Set(GrievanceDocument.Fields.NysRP525IsReducedValue, answers.Admin_Rp525_Total)
                        .Set(GrievanceDocument.Fields.NysRP525IsNotReduced, answers.Admin_Rp525_Check2b)
                        .Set(GrievanceDocument.Fields.BarReviewed, isComplete)
                        .Set(GrievanceDocument.Fields.BarReviewDate, isComplete ? DateTime.Now.ToString() : null)
                        .Set(GrievanceDocument.Fields.BarReviewDateUnix, isComplete ? TimeService.GetUnixTimestampInMilliseconds(DateTime.UtcNow) : 0);
            collection
                .UpdateOne(filter, update);
        }

        /// <summary>
        /// INTENT: Can be used to mark a grievance as reviewed or not reviewed
        /// </summary>        
        public void UpdateBarReviewStatus(
            IMongoCollection<BsonDocument> collection,
            string guidString,
            bool isBarReviewed
        )
        {
            Contract.Requires(collection != null);
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, guidString);

            // Our Cloud Run containers (based on Alpine) appear to not have timezones installed?
            //var timeUtc = DateTime.UtcNow; 
            //TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set("barReviewed", isBarReviewed)
                        .Set("barReviewDate", (isBarReviewed) ? DateTime.UtcNow.ToString() : "")
                        .Set("barReviewDateUnix", (isBarReviewed) ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : -1);

            collection
                .UpdateOne(filter, update);
        }

        public async Task UpdateDispositionGenerationStatus(
            IMongoCollection<BsonDocument> grievancesCollection, 
            ImmutableList<string> grievanceIds,
            bool setStatusToTrue
        )
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.In(GrievanceDocument.Fields.GuidString, grievanceIds);

            // Our Cloud Run containers (based on Alpine) appear to not have timezones installed?
            //var timeUtc = DateTime.UtcNow; 
            //TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.DispositionEmailSent, setStatusToTrue)
                        .Set(GrievanceDocument.Fields.DispositionEmailSentDate, DateTime.UtcNow.ToString());

            await grievancesCollection
                .UpdateManyAsync(filter, update);
        }

        public BsonArray BuildBsonArray<T>(List<T> objects)
        {
            var objectsBsonArray = new BsonArray();
            objects.ForEach((s) => { objectsBsonArray.Add(s.ToBsonDocument());});
            return objectsBsonArray;
        }
    }
}
