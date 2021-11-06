using Library.Models;
using Library.Models.DataTransfer;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database
{
    public interface IDocumentDatabase
    {
        BsonArray BuildBsonArray<T>(List<T> objects);

        Task DeleteGrievanceSoftly(
            DocumentDatabaseSettings dbSettings,
            string grievanceId
        );
        Task<IEnumerable<string>> GetAllGrievanceIds(
            IMongoCollection<BsonDocument> grievanceCollection
        );
        ProjectionDefinition<BsonDocument> BuildProjection(ImmutableList<string> fieldsToInclude);
        
        List<BsonDocument> GetChangeList(IMongoCollection<BsonDocument> collection, DateTime dateFilterStart, DateTime dateFilterEnd);

        Task ArchiveDeletedGrievances(
            IMongoCollection<BsonDocument> submissionsCollection,
            IMongoCollection<BsonDocument> submissionsArchiveCollection
        );
        IEnumerable<GrievanceApplication> GetAllGrievances(IMongoCollection<BsonDocument> collection);
        IMongoCollection<BsonDocument> GetCollection(string collectionName);
        ConflictingSubmittersInfo GetConflictingSubmitters(IDocumentDatabase client, string taxMapId, DocumentDatabaseSettings settings);
        List<BsonDocument> GetDocsByTaxMapId(IMongoCollection<BsonDocument> collection, string taxMapId);
        BsonDocument GetDocumentByGuid(IMongoCollection<BsonDocument> collection, string grievanceId);
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

        IEnumerable<GrievanceMissingRp524> GetDocumentsByField<T>(
            IMongoCollection<BsonDocument> collection,
            string field1Name,
            IEnumerable<T> field1Value
        );

        List<BsonDocument> GetDocumentsByTwoFields<T, U>(
            IMongoCollection<BsonDocument> collection,
            ProjectionDefinition<BsonDocument> projection,
            string field1Name,
            T field1Value,
            string field2Name,
            U field2Value,
            bool field2ValueEquals = true
        );
        
        BsonDocument GetDocumentByTwoStringFields(IMongoCollection<BsonDocument> collection, string field1Name, string field1Value, string field2Name, string field2Value);
        List<BsonDocument> GetDocumentsByStringField(IMongoCollection<BsonDocument> collection, ProjectionDefinition<BsonDocument> projection, string fieldName, string fieldValue);

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

        void InsertGrievance(
            string submissionGuid, 
            string taxMapId, 
            string applicantEmail, 
            IDocumentDatabase dbClient, 
            DocumentDatabaseSettings settings, 
            string creationMechanism, 
            string complaintType, 
            string proposedValue, 
            bool isPersonalHearingRequested, 
            bool includesConflictOfInterest, 
            bool includesResQuestionnaire, 
            bool includesComQuestionnaire,
            bool includesLetterOfAuth,
            bool includesIncomeExpenseForms,
            bool includesIncomeExpenseExclusion,
            bool includesSupportingDocumentation,
            string signatureType,
            string complainant,
            string attorneyGroup,
            string attorneyPhone,
            string attorneyDataRaw,
            string complainantMailAddress,
            string coOpUnitNum,
            string reason,
            string notes
        );
        void UpdateBarReviewStatus(IMongoCollection<BsonDocument> collection, string guidString, bool isBarReviewed);
        void UpdateNysRp525Answers(IMongoCollection<BsonDocument> collection, string submissionId, NysRps525OnlineFormAnswers answers, bool isComplete);
        NysRps525OnlineFormAnswers GetNysRp525Answers(IMongoCollection<BsonDocument> collection, string submissionId);

        void UpdateReviewStatus(IMongoCollection<BsonDocument> collection, string grievanceId, bool reviewed);
        
        void UpdateIsRequestingPersonalHearing(
            IMongoCollection<BsonDocument> collection, 
            string grievanceId, 
            bool isRequestingPersonalHearing
        );
        Task UpdateDocumentField<T>(
            IMongoCollection<BsonDocument> collection,
            string idFieldName,
            string documentId,
            string fieldToUpdate,
            T newFieldValue
        );

        /// <summary>
        /// DEPRECATED: Use this.UpdateDocumentField() instead.
        /// </summary>        
        void UpdateCompletedPersonalHearing(
            IMongoCollection<BsonDocument> collection,
            string grievanceId,
            bool isHearingCompleted
        );

        Task UpdateGrievance(
            IMongoCollection<BsonDocument> collection,
            GrievanceApplication grievance
        );

        Task UpdateDispositionGenerationStatus(
            IMongoCollection<BsonDocument> grievancesCollection,
            ImmutableList<string> grievanceIds,
            bool setStatusToTrue
        );

        public void UpdateIncludesFileTypeFields(
            IMongoCollection<BsonDocument> collection,
            string grievanceId,
            bool includesPersonalHearing,
            bool includesConflictOfInterest,
            bool includesResQuestionnaire,
            bool includesComQuestionnaire,
            bool includesLetterOfAuthorization,
            bool includesIncomeExpenseForms,
            bool includesIncomeExpenseExclusion,
            bool includesSupportingDocumentation
        );
    }
}