using Library.Models;
using Library.Models.DataTransfer;
using Library.Models.NoSQLDatabaseSchema;
using Library.Models.RP_524;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Library.Services.Time;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database
{
    public class MongoDatabase : IDocumentDatabase
    {
        private readonly MongoClient _client;

        private readonly FilterDefinition<BsonDocument> _submissionNotDeletedFilter =
            Builders<BsonDocument>.Filter
                .Ne(GrievanceDocument.Fields.IsDeleted, true);

        private readonly FilterDefinition<BsonDocument> _submissionBarReviewedFilter =
            Builders<BsonDocument>.Filter
                .Eq(GrievanceDocument.Fields.BarReviewed, true);

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
            // &readPreference = primary & readConcernLevel = local
            return $"mongodb+srv://{dbUser}:{password}@{server}/{dbName}?retryWrites=true&w=majority";
        }

        public IMongoCollection<BsonDocument> GetCollection(string collectionName)
        {
            var database = _client.GetDatabase(_databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        public async Task<IEnumerable<string>> GetAllSubmissionIds(IMongoCollection<BsonDocument> grievanceCollection)
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
            DateTime dateFilterEnd
        )
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
        public IEnumerable<GrievanceApplication> GetAllGrievances(IMongoCollection<BsonDocument> collection)
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
                collection
                    .Find(doc => true) //_submissionNotDeletedFilter)
                    .Project(projection)
                    .ToList();
            
            return documents
                .Select(d => BsonSerializer.Deserialize<GrievanceApplication>(d));
        }

        public async Task ArchiveDeletedDocs(
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

        /// <param name="fieldsToInclude">list of fields to include in the projection</param>
        /// <returns></returns>
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

        [Obsolete("DEPRECATED. Use a generic method instead.")]
        public List<BsonDocument> GetDocsByTaxMapId(
            IMongoCollection<BsonDocument> collection,
            string taxMapId
        )
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

        public ConflictingSubmittersInfo GetConflictingSubmitters(
            IDocumentDatabase client,
            string taxMapId,
            DocumentDatabaseSettings settings
        )
        {
            var collection = client.GetCollection(settings.GrievancesCollectionName);
            List<BsonDocument> docs = client.GetDocsByTaxMapId(
                collection,
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
        [Obsolete("Please use a generic method instead")]
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

        [Obsolete("DEPRECATED. Use a generic method instead.")]
        public BsonDocument GetDocumentByGuid(
            IMongoCollection<BsonDocument> collection,
            string guid
        )
        {
            var filter = Builders<BsonDocument>.Filter.Eq(GrievanceDocument.Fields.GuidString, guid);
            ProjectionDefinition<BsonDocument> projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocument.Fields.TaxMapId)
                    .Exclude(GrievanceDocument.Fields.InternalDatabaseId);
            var document = collection.Find(filter).Project(projection).FirstOrDefault();
            return document;
        }

        /// <summary>
        /// If you update this method, also update GetAllDocuments
        /// </summary>
        public void InsertGrievance(
            string submissionGuid,
            string taxMapId,
            string applicantEmail,
            IDocumentDatabase mongoClient,
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
        )
        {
            Contract.Requires(mongoClient != null && settings != null);

            var collection = mongoClient.GetCollection(collectionName: settings.GrievancesCollectionName);

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
            collection.InsertOne(document);        
        }

        public Task InsertDocument(
            IMongoCollection<BsonDocument> collection,
            BsonDocument document)
        {
            return collection.InsertOneAsync(document);
        }

        public async Task DeleteGrievanceSoftly(
            DocumentDatabaseSettings dbSettings,
            string grievanceId
        )
        {
            var submissionsCollection = GetCollection(collectionName: dbSettings.GrievancesCollectionName);
            var submissionsArchiveCollection = GetCollection($"{dbSettings.GrievancesCollectionName}_deleted");

            FilterDefinition<BsonDocument> filterDef = 
                Builders<BsonDocument>.Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);

            UpdateDefinition<BsonDocument> updateDef =
                new BsonDocument("$set", new BsonDocument(GrievanceDocument.Fields.IsDeleted, true));

            await submissionsCollection.UpdateOneAsync(filterDef, updateDef);

            await ArchiveDeletedDocs(submissionsCollection, submissionsArchiveCollection);
        }

        public async Task UpdateDocumentField<T>(
            IMongoCollection<BsonDocument> collection, 
            string documentId, 
            string fieldToUpdate, 
            T newfieldValue
        ) {
            Contract.Requires(collection != null);
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, documentId);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(fieldToUpdate, newfieldValue);

            await collection.UpdateOneAsync(
                filter, update
            ).ConfigureAwait(false);
        }

        public void UpdateIsRequestingPersonalHearing(
            IMongoCollection<BsonDocument> collection, 
            string grievanceId, 
            bool isRequestingPersonalHearing
        )
        {
            Contract.Requires(collection != null);
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, grievanceId);
            
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set("requested_personal_hearing", isRequestingPersonalHearing);

            collection
                .UpdateOne(filter, update);
        }
        public void UpdateCompletedPersonalHearing(
            IMongoCollection<BsonDocument> collection,
            string grievanceId,
            bool isHearingCompleted
        )
        {
            Contract.Requires(collection != null);
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

        /// <summary>
        /// TODO: Unit test (this is broken)(<== why didn't I write a better comment?)
        /// </summary>
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
            Contract.Requires(collection != null);
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
            bool reviewed
        )
        {
            Contract.Requires(collection != null);
            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocument.Fields.GuidString, guid);

            // Our Cloud Run containers (based on Alpine) appear to not have timezones installed?
            // var timeUtc = DateTime.UtcNow; 
            // TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            // DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocument.Fields.Downloaded, reviewed)
                        .Set(GrievanceDocument.Fields.DownloadDate, (reviewed) ? DateTime.UtcNow.ToString() : "")
                        .Set(GrievanceDocument.Fields.DownloadDateUnix, (reviewed) ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : -1);

            collection
                .UpdateOne(filter, update);
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

        public BsonArray BuildBsonArray<T>(List<T> objects)
        {
            var objectsBsonArray = new BsonArray();
            objects.ForEach((s) => { objectsBsonArray.Add(s.ToBsonDocument());});
            return objectsBsonArray;
        }
    }
}
