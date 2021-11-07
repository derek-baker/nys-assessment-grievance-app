using Library.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database
{
    public interface IDocumentDatabase
    {
        BsonArray BuildBsonArray<T>(
            List<T> objects);

        ProjectionDefinition<BsonDocument> BuildProjection(
            ImmutableList<string> fieldsToInclude);
        
        IMongoCollection<BsonDocument> GetCollection(
            string collectionName);
        
        BsonDocument GetDocumentByStringField(
            IMongoCollection<BsonDocument> collection, 
            ProjectionDefinition<BsonDocument> projection, 
            string fieldName, 
            string fieldValue
        );

        Task<IAsyncCursor<BsonDocument>> GetDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection);

        Task<IAsyncCursor<BsonDocument>> GetDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            FilterDefinition<BsonDocument> filter);

        Task<IAsyncCursor<BsonDocument>> GetSortedDocuments(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            FilterDefinition<BsonDocument> filter,
            SortDefinition<BsonDocument> sort);

        List<BsonDocument> GetDocumentsByTwoFields<T, U>(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string field1Name,
            T field1Value,
            string field2Name,
            U field2Value,
            bool field2ValueEquals = true
        );
        
        BsonDocument GetDocumentByTwoStringFields(
            IMongoCollection<BsonDocument> collection, 
            string field1Name, 
            string field1Value, 
            string field2Name, 
            string field2Value);

        List<BsonDocument> GetDocumentsByStringField(
            IMongoCollection<BsonDocument> collection, 
            ProjectionDefinition<BsonDocument> projection, 
            string fieldName, 
            string fieldValue);

        List<BsonDocument> GetDocumentsByStringFieldCaseInsensitive(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string fieldName,
            string fieldValue
        );

        Task InsertDocument(
            IMongoCollection<BsonDocument> collection,
            BsonDocument document
        );

        Task UpdateDocumentField<T>(
            IMongoCollection<BsonDocument> collection,
            string idFieldName,
            string documentId,
            string fieldToUpdate,
            T newFieldValue
        );

        void UpdateBarReviewStatus(IMongoCollection<BsonDocument> collection, string guidString, bool isBarReviewed);
        void UpdateNysRp525Answers(IMongoCollection<BsonDocument> collection, string submissionId, NysRps525OnlineFormAnswers answers, bool isComplete);
        NysRps525OnlineFormAnswers GetNysRp525Answers(IMongoCollection<BsonDocument> collection, string submissionId);
    }
}