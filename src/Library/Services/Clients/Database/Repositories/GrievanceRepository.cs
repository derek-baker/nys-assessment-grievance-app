using Library.Models;
using Library.Models.DataTransfer;
using Library.Models.NoSQLDatabaseSchema;
using Library.Models.RP_524;
using Library.Services.Time;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public class GrievanceRepository
    {
        private readonly IDocumentDatabase _db;
        private readonly IMongoCollection<BsonDocument> _collection;

        private readonly FilterDefinition<BsonDocument> _submissionNotDeletedFilter =
            Builders<BsonDocument>.Filter
                .Ne(GrievanceDocument.Fields.IsDeleted, true);

        private readonly FilterDefinition<BsonDocument> _submissionBarReviewedFilter =
            Builders<BsonDocument>.Filter
                .Eq(GrievanceDocument.Fields.BarReviewed, true);

        public GrievanceRepository(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            _db = db;
            _collection = _db.GetCollection(dbSettings.GrievancesCollectionName);
        }

        public async Task<IEnumerable<string>> GetAllGrievanceIds(IMongoCollection<BsonDocument> grievanceCollection)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                        .Include(GrievanceDocument.Fields.GuidString)
                        .Exclude(GrievanceDocument.Fields.InternalDatabaseId);

            var documents =
                await grievanceCollection
                    .Find(doc => true)
                    .Project(projection)
                    .ToListAsync();

            var output = documents.Select(
                d => BsonSerializer.Deserialize<GrievanceId>(d)
            ).Select(g => g.guid);
            return output;
        }

        public List<BsonDocument> GetChangeList(
            IMongoCollection<BsonDocument> collection,
            DateTime dateFilterStart,
            DateTime dateFilterEnd)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocument.Fields.GuidString)
                    .Include(GrievanceDocument.Fields.TaxMapId)
                    .Include(GrievanceDocument.Fields.Complainant)
                    .Include(GrievanceDocument.Fields.AttorneyGroup)
                    .Include(GrievanceDocument.Fields.ComplainantMailAddress)
                    .Include(GrievanceDocument.Fields.CoOpUnitNum)
                    .Include(GrievanceDocument.Fields.NysRP525Tentative)
                    .Include(GrievanceDocument.Fields.ComplaintType)
                    .Include(GrievanceDocument.Fields.NysRP525Answers)
                    .Include(GrievanceDocument.Fields.BarReviewDate)
                    .Include(GrievanceDocument.Fields.Reason)

                    .Exclude(GrievanceDocument.Fields.InternalDatabaseId);

            FilterDefinition<BsonDocument> barReviewStartDateFilter =
                Builders<BsonDocument>.Filter
                    .Gte(GrievanceDocument.Fields.BarReviewDateUnix, TimeService.GetUnixTimestampInMilliseconds(dateFilterStart));

            FilterDefinition<BsonDocument> barReviewEndDateFilter =
                Builders<BsonDocument>.Filter
                    .Lte(GrievanceDocument.Fields.BarReviewDateUnix, TimeService.GetUnixTimestampInMilliseconds(dateFilterEnd));

            var documents =
                collection
                    .Find(
                        _submissionNotDeletedFilter
                        & _submissionBarReviewedFilter
                        & barReviewStartDateFilter
                        & barReviewEndDateFilter
                    )
                    .Project(projection)
                    .ToList();

            return documents;
        }

        //public async Task AddIdentityIdIntsToAllDocs(IMongoCollection<BsonDocument> collection)
        //{
        //    const int deletedId = -1;
        //    int paginationId = 1;

        //    var cursor = collection.Find(_submissionNotDeletedFilter).ToCursor();
        //    foreach (var doc in cursor.ToEnumerable())
        //    {
        //        await UpdateDocumentField(
        //            collection: collection,
        //            documentId: doc["guid"].AsString,
        //            fieldToUpdate: "pagination_id",
        //            newfieldValue: paginationId
        //        );
        //        paginationId++;
        //    }

        //    var deletedFilter = Builders<BsonDocument>
        //        .Filter
        //        .Eq(GrievanceDocument.Fields.IsDeleted, true);

        //    var deletedCursor = collection.Find(deletedFilter).ToCursor();
        //    foreach (var doc in deletedCursor.ToEnumerable())
        //    {
        //        await UpdateDocumentField(
        //            collection: collection,
        //            documentId: doc["guid"].AsString,
        //            fieldToUpdate: "pagination_id",
        //            newfieldValue: deletedId
        //        );
        //        paginationId++;
        //    }
        //}

        //public async Task AddRepGroupEmailFieldToAllDocs(IMongoCollection<BsonDocument> collection, List<RepGroupInfo> repGroupInfos)
        //{
        //    var cursor = collection.Find(x => true).ToCursor();
        //    foreach (var doc in cursor.ToEnumerable())
        //    {
        //        var submitterEmail = doc["email"].ToString();
        //        var repGroupInfo = repGroupInfos.Where(i => i.GroupName1 == doc.GetValue("attorney_group", "default")).FirstOrDefault();
        //        var repGroupEmail = repGroupInfo?.Email;
        //        var emailToUse = (repGroupEmail != null && !string.IsNullOrWhiteSpace(repGroupEmail)) 
        //            ? repGroupEmail
        //            : submitterEmail;

        //        await UpdateDocumentField(
        //            collection: collection,
        //            documentId: doc["guid"].AsString,
        //            fieldToUpdate: "attorney_email",
        //            newfieldValue: emailToUse
        //        );
        //    }
        //}

        /// <summary>
        /// TODO: Extend this with more fields so we can export more data to CSV
        /// TODO: Set max limit? And pagination?
        /// </summary>        
        public IEnumerable<GrievanceApplication> GetAll()
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocument.Fields.GuidString)
                    .Include(GrievanceDocument.Fields.TaxMapId)
                    .Include(GrievanceDocument.Fields.Email)
                    .Include(GrievanceDocument.Fields.SubmitDate)

                    .Include(GrievanceDocument.Fields.RequestedPersonalHearing)
                    .Include(GrievanceDocument.Fields.CompletedPersonalHearing)

                    .Include(GrievanceDocument.Fields.ComplaintType)
                    .Include(GrievanceDocument.Fields.ProposedValue)
                    .Include(GrievanceDocument.Fields.CreatorName)

                    .Include(GrievanceDocument.Fields.Downloaded)
                    .Include(GrievanceDocument.Fields.DownloadDate)
                    .Include(GrievanceDocument.Fields.BarReviewed)
                    .Include(GrievanceDocument.Fields.BarReviewDate)
                    .Include(GrievanceDocument.Fields.DispositionEmailSent)
                    .Include(GrievanceDocument.Fields.DispositionEmailSentDate)

                    .Include(GrievanceDocument.Fields.IncludesConflict)
                    .Include(GrievanceDocument.Fields.IncludesResQuestionnaire)
                    .Include(GrievanceDocument.Fields.IncludesComQuestionnaire)
                    .Include(GrievanceDocument.Fields.IncludesLetterOfAuth)
                    .Include(GrievanceDocument.Fields.IncludesIncomeExpenseForms)
                    .Include(GrievanceDocument.Fields.IncludesIncomeExpenseExclusion)
                    .Include(GrievanceDocument.Fields.IncludesSupportingDocumentation)
                    .Include(GrievanceDocument.Fields.FiveSignatureType)

                    .Include(GrievanceDocument.Fields.Complainant)
                    .Include(GrievanceDocument.Fields.AttorneyGroup)
                    .Include(GrievanceDocument.Fields.AttorneyEmail)
                    .Include(GrievanceDocument.Fields.AttorneyPhone)
                    .Include(GrievanceDocument.Fields.AttorneyDataRaw)
                    .Include(GrievanceDocument.Fields.ComplainantMailAddress)
                    .Include(GrievanceDocument.Fields.CoOpUnitNum)
                    .Include(GrievanceDocument.Fields.Reason)
                    .Include(GrievanceDocument.Fields.Notes)

                    .Include(GrievanceDocument.Fields.NysRP525Tentative)
                    .Include(GrievanceDocument.Fields.NysRP525IsReduced)
                    .Include(GrievanceDocument.Fields.NysRP525IsReducedValue)
                    .Include(GrievanceDocument.Fields.NysRP525Answers)

                    .Exclude(GrievanceDocument.Fields.InternalDatabaseId);

            var documents =
                _collection
                    .Find(doc => true)
                    .Project(projection)
                    .ToList();

            return documents
                .Select(d => BsonSerializer.Deserialize<GrievanceApplication>(d));
        }

        public async Task ArchiveDeletedGrievances(
            IMongoCollection<BsonDocument> submissionsCollection,
            IMongoCollection<BsonDocument> submissionsArchiveCollection)
        {
            var deletedFilter = Builders<BsonDocument>
                .Filter.Eq(GrievanceDocument.Fields.IsDeleted, true);

            var deletedDocuments = submissionsCollection
                .Find(deletedFilter)
                .ToEnumerable();

            await submissionsArchiveCollection.InsertManyAsync(deletedDocuments);

            await submissionsCollection.DeleteManyAsync(deletedFilter);
        }

        public List<BsonDocument> GetDocsByTaxMapId(
            IMongoCollection<BsonDocument> collection,
            string taxMapId)
        {
            ProjectionDefinition<BsonDocument> projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocument.Fields.TaxMapId)
                    .Include("email")
                    .Include("submit_date")
                    .Exclude(GrievanceDocument.Fields.InternalDatabaseId);

            var taxIdFilter = Builders<BsonDocument>.Filter.Eq(GrievanceDocument.Fields.TaxMapId, taxMapId);
            var filter = taxIdFilter & _submissionNotDeletedFilter;
            var cursor = collection.Find(filter).Project(projection).ToCursor();
            return cursor.ToList();
        }

        public ConflictingSubmittersInfo GetConflictingSubmitters(string taxMapId)
        {
            List<BsonDocument> docs = GetDocsByTaxMapId(
                _collection,
                taxMapId
            );
            string emailSubmitters = "";
            var applications = new List<GrievanceApplication>();
            if (docs.Count > 0)
            {
                docs.ForEach(
                    doc =>
                    {
                        var application = BsonSerializer.Deserialize<GrievanceApplication>(doc);
                        emailSubmitters += $"{application.email} - {application.submit_date} \n";
                        applications.Add(
                            BsonSerializer.Deserialize<GrievanceApplication>(doc)
                        );
                    }
                );
            }
            return new ConflictingSubmittersInfo(applications, emailSubmitters);
        }

        public IEnumerable<GrievanceMissingRp524> GetDocumentsByField<T>(
            IMongoCollection<BsonDocument> collection,
            string field1Name,
            IEnumerable<T> values)
        {
            var filter = Builders<BsonDocument>.Filter.In(field1Name, values);
            var documents = collection.Find(filter).ToCursor().ToEnumerable();

            var data = documents.Select(d => BsonSerializer.Deserialize<GrievanceMissingRp524>(d));
            return data;
        }

        public BsonDocument GetByGuid(
            string guid)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(GrievanceDocument.Fields.GuidString, guid);
            ProjectionDefinition<BsonDocument> projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocument.Fields.TaxMapId)
                    .Exclude(GrievanceDocument.Fields.InternalDatabaseId);
            var document = _collection.Find(filter).Project(projection).FirstOrDefault();
            return document;
        }

        /// <summary>
        /// If you update this method, also update GetAllDocuments
        /// </summary>
        public void InsertGrievance(
            string submissionGuid,
            string taxMapId,
            string applicantEmail,
            //IDocumentDatabase mongoClient,
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
            string notes)
        {
            //var collection = mongoClient.GetCollection(collectionName: settings.GrievancesCollectionName);

            var document = new BsonDocument {
                { GrievanceDocument.Fields.GuidString, submissionGuid },
                { GrievanceDocument.Fields.TaxMapId, taxMapId },
                { "complaint_type", complaintType },
                { "proposed_value", proposedValue },
                { "email", applicantEmail },
                { "creator_name", creationMechanism },
                { "submit_date", DateTime.Now.ToString() },
                { "downloaded", false },
                { "download_date", "" },
                { "download_date_unix", -1 },
                { "requested_personal_hearing", isPersonalHearingRequested },
                { "completed_personal_hearing", false },
                { "includes_conflict", includesConflictOfInterest },
                { "includes_res_questionnaire", includesResQuestionnaire },
                { "includes_com_questionnaire", includesComQuestionnaire },
                { "includes_letter_of_auth", includesLetterOfAuth },
                { GrievanceDocument.Fields.IncludesIncomeExpenseForms, includesIncomeExpenseForms },
                { GrievanceDocument.Fields.IncludesIncomeExpenseExclusion, includesIncomeExpenseExclusion },
                { GrievanceDocument.Fields.IncludesSupportingDocumentation, includesSupportingDocumentation },
                { "five_signature_type", signatureType },

                { "complainant", complainant },
                { "attorney_group", attorneyGroup },
                { "attorney_phone", attorneyPhone },
                { GrievanceDocument.Fields.AttorneyDataRaw, attorneyDataRaw },
                { "complainant_mail_address", complainantMailAddress },
                { "co_op_unit_num", coOpUnitNum },
                { "reason", reason },
                { "notes", notes}
                //{ GrievanceDocument.Fields.PaginationId, maxId }
            };
            _collection.InsertOne(document);
        }

        public async Task DeleteGrievanceSoftly(
            DocumentDatabaseSettings dbSettings,
            string grievanceId
        )
        {
            var submissionsCollection = _db.GetCollection(collectionName: dbSettings.GrievancesCollectionName);
            var submissionsArchiveCollection = _db.GetCollection($"{dbSettings.GrievancesCollectionName}_deleted");

            FilterDefinition<BsonDocument> filterDef =
                Builders<BsonDocument>.Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);

            UpdateDefinition<BsonDocument> updateDef =
                new BsonDocument("$set", new BsonDocument(GrievanceDocument.Fields.IsDeleted, true));

            await submissionsCollection.UpdateOneAsync(filterDef, updateDef);

            await ArchiveDeletedGrievances(submissionsCollection, submissionsArchiveCollection);
        }

        public void UpdateIsRequestingPersonalHearing(string grievanceId, bool isRequestingPersonalHearing)
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set("requested_personal_hearing", isRequestingPersonalHearing);

            _collection
                .UpdateOne(filter, update);
        }
        public void UpdateCompletedPersonalHearing(
            IMongoCollection<BsonDocument> collection,
            string grievanceId,
            bool isHearingCompleted
        )
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set("completed_personal_hearing", isHearingCompleted);

            collection
                .UpdateOne(filter, update);
        }

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
            bool includesSupportingDocumentation)
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);

            if (includesPersonalHearing)
            {
                var update =
                    Builders<BsonDocument>
                        .Update
                            .Set("requested_personal_hearing", includesPersonalHearing);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesConflictOfInterest)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set("includes_conflict", includesConflictOfInterest);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesResQuestionnaire)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set("includes_res_questionnaire", includesResQuestionnaire);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesComQuestionnaire)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set("includes_com_questionnaire", includesComQuestionnaire);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesLetterOfAuthorization)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set("includes_letter_of_auth", includesLetterOfAuthorization);
                collection
                    .UpdateOne(filter, update);
            }

            if (includesIncomeExpenseForms)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.IncludesIncomeExpenseForms, includesIncomeExpenseForms);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesIncomeExpenseExclusion)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.IncludesIncomeExpenseExclusion, includesIncomeExpenseExclusion);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesSupportingDocumentation)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.IncludesSupportingDocumentation, includesSupportingDocumentation);
                collection
                    .UpdateOne(filter, update);
            }
        }

        /// <summary>
        /// INTENT: Can be used to mark a grievance application as downloaded or not downloaded
        /// TODO: Fix method name
        /// </summary>        
        public void UpdateReviewStatus(
            IMongoCollection<BsonDocument> collection,
            string guid,
            bool reviewed)
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, guid);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.Downloaded, reviewed)
                        .Set(GrievanceDocument.Fields.DownloadDate, (reviewed) ? DateTime.UtcNow.ToString() : "")
                        .Set(GrievanceDocument.Fields.DownloadDateUnix, (reviewed) ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : -1);

            collection
                .UpdateOne(filter, update);
        }

        public async Task UpdateGrievance(
            IMongoCollection<BsonDocument> collection,
            GrievanceApplication grievance
        )
        {
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, grievance.guid);

            // WARNING: This is rigid in the sense that it assumes all the data we want is public
            var dict =
                grievance.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToDictionary(
                            prop => prop.Name,
                            prop => prop.GetValue(grievance, null)
                        );

            var doc = new BsonDocument(dict);
            await collection.ReplaceOneAsync(
                filter: filter,
                replacement: doc
            ).ConfigureAwait(false);
        }

        public async Task UpdateIncludesDocumentFields(
            string grievanceId,
            bool includesConflictOfInterest,
            bool includesResQuestionnaire,
            bool includesComQuestionnaire,
            bool includesLetterOfAuthorization,
            bool includesIncomeExpenseForms,
            bool includesIncomeExpenseExclusion,
            bool includesSupportingDocumentation)
        {
            if (includesConflictOfInterest)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: "includes_conflict",
                    newFieldValue: includesConflictOfInterest
                );
            }
            if (includesResQuestionnaire)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: "includes_res_questionnaire",
                    newFieldValue: includesResQuestionnaire
                );
            }
            if (includesComQuestionnaire)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: "includes_com_questionnaire",
                    newFieldValue: includesComQuestionnaire
                );
            }
            if (includesLetterOfAuthorization)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: "includes_letter_of_auth",
                    newFieldValue: includesLetterOfAuthorization
                );
            }

            if (includesIncomeExpenseForms)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: GrievanceDocument.Fields.IncludesIncomeExpenseForms,
                    newFieldValue: includesIncomeExpenseForms
                );
            }
            if (includesIncomeExpenseExclusion)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: GrievanceDocument.Fields.IncludesIncomeExpenseExclusion,
                    newFieldValue: includesIncomeExpenseExclusion
                );
            }
            if (includesSupportingDocumentation)
            {
                await _db.UpdateDocumentField(
                    collection: _collection,
                    idFieldName: GrievanceDocument.Fields.GuidString,
                    documentId: grievanceId,
                    fieldToUpdate: GrievanceDocument.Fields.IncludesSupportingDocumentation,
                    newFieldValue: includesSupportingDocumentation
                );
            }
        }

        public bool TestGuidExistence(string guidString)
        {
            var documentAssociatedWithGuid = GetByGuid(guidString);
            return documentAssociatedWithGuid != null;
        }

        public string GetNewGuid()
        {
            bool uniqueGuidGenerated = false;
            string guid = "";

            string guidFieldNameInCollection = "guid";

            while (uniqueGuidGenerated == false)
            {
                string potentialGuid = System.Guid.NewGuid().ToString();
                var docAssociatedWithGuid = _db.GetDocumentByStringField(
                    _collection,
                    projection: _db.BuildProjection(
                        (new List<string>() { guidFieldNameInCollection }).ToImmutableList()
                    ),
                    fieldName: guidFieldNameInCollection,
                    fieldValue: guid
                );
                if (docAssociatedWithGuid == null)
                {
                    guid = potentialGuid;
                    uniqueGuidGenerated = true;
                }
            }
            return guid;
        }
    }
}
