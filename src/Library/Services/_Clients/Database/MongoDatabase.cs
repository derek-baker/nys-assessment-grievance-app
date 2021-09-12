using Library.Models;
using Library.Models.DataTransfer;
using Library.Models.NoSQLDatabaseSchema;
using Library.Models.RP_524;
using Library.Models.Settings;
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

namespace Library.Database
{
    public class MongoDatabase : IDocumentDatabase
    {
        private readonly MongoClient _client;

        private readonly FilterDefinition<BsonDocument> _submissionNotDeletedFilter =
            Builders<BsonDocument>.Filter
                .Ne(GrievanceDocumentFields.IsDeleted, true);

        private readonly FilterDefinition<BsonDocument> _submissionBarReviewedFilter =
            Builders<BsonDocument>.Filter
                .Eq(GrievanceDocumentFields.BarReviewed, true);

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
                        .Include(GrievanceDocumentFields.GuidString)
                        .Exclude(GrievanceDocumentFields.InternalDatabaseId);

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
                    .Include(GrievanceDocumentFields.GuidString)
                    .Include(GrievanceDocumentFields.TaxMapId)
                    .Include(GrievanceDocumentFields.Complainant)
                    .Include(GrievanceDocumentFields.AttorneyGroup)
                    .Include(GrievanceDocumentFields.ComplainantMailAddress)
                    .Include(GrievanceDocumentFields.CoOpUnitNum)
                    .Include(GrievanceDocumentFields.NysRP525Tentative)
                    .Include(GrievanceDocumentFields.ComplaintType)
                    .Include(GrievanceDocumentFields.NysRP525Answers)
                    .Include(GrievanceDocumentFields.BarReviewDate)
                    .Include(GrievanceDocumentFields.Reason)

                    .Exclude(GrievanceDocumentFields.InternalDatabaseId);

            FilterDefinition<BsonDocument> barReviewStartDateFilter =
                Builders<BsonDocument>.Filter
                    .Gte(GrievanceDocumentFields.BarReviewDateUnix, TimeService.GetUnixTimestampInMilliseconds(dateFilterStart));

            FilterDefinition<BsonDocument> barReviewEndDateFilter =
                Builders<BsonDocument>.Filter
                    .Lte(GrievanceDocumentFields.BarReviewDateUnix, TimeService.GetUnixTimestampInMilliseconds(dateFilterEnd));

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
        //        .Eq(GrievanceDocumentFields.IsDeleted, true);

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

        public async Task<IEnumerable<RepGroupInfo>> GetRepresentatives(IMongoCollection<BsonDocument> collection)
        {
            var documents = await collection.FindAsync(doc => true);

            var reps = (await documents.ToListAsync())
                .Select(d => BsonSerializer.Deserialize<RepGroupInfoDocument>(d));

            return reps.FirstOrDefault()?.Representatives;
        }

        public async Task SetRepresentatives(IMongoCollection<BsonDocument> collection, IEnumerable<RepGroupInfo> reps)
        {
            await collection.DeleteManyAsync(doc => true);

            var document = new BsonDocument
            {
                { RepresentativeDocumentFields.Representatives, BuildBsonArray(reps.ToList()) }
            };
            await InsertDocument(collection, document);
        }

        public async Task<UserSettings> GetUserSettings(IMongoCollection<BsonDocument> collection)
        {
            var documents = await collection.FindAsync(doc => true);

            var reps = (await documents.ToListAsync())
                .Select(d => BsonSerializer.Deserialize<UserSettingsDocument>(d));

            return reps.FirstOrDefault()?.UserSettings;
        }

        public async Task SetUserSettings(IMongoCollection<BsonDocument> collection, UserSettings settings)
        {
            await collection.DeleteManyAsync(doc => true);
            var document = new BsonDocument
            {
                { UserSettingsDocumentFields.UserSettings, settings.ToBsonDocument() }
            };
            await InsertDocument(collection, document);
        }

        /// <summary>
        /// TODO: Extend this with more fields so we can export more data to CSV
        /// TODO: Set max limit? And pagination?
        /// </summary>        
        public IEnumerable<GrievanceApplication> GetAllGrievances(IMongoCollection<BsonDocument> collection)
        {
            var projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocumentFields.GuidString)
                    .Include(GrievanceDocumentFields.TaxMapId)
                    .Include(GrievanceDocumentFields.Email)
                    .Include(GrievanceDocumentFields.SubmitDate)

                    .Include(GrievanceDocumentFields.RequestedPersonalHearing)
                    .Include(GrievanceDocumentFields.CompletedPersonalHearing)

                    .Include(GrievanceDocumentFields.ComplaintType)
                    .Include(GrievanceDocumentFields.ProposedValue)
                    .Include(GrievanceDocumentFields.CreatorName)

                    .Include(GrievanceDocumentFields.Downloaded)
                    .Include(GrievanceDocumentFields.DownloadDate)
                    .Include(GrievanceDocumentFields.BarReviewed)
                    .Include(GrievanceDocumentFields.BarReviewDate)
                    .Include(GrievanceDocumentFields.DispositionEmailSent)
                    .Include(GrievanceDocumentFields.DispositionEmailSentDate)

                    .Include(GrievanceDocumentFields.IncludesConflict)
                    .Include(GrievanceDocumentFields.IncludesResQuestionnaire)
                    .Include(GrievanceDocumentFields.IncludesComQuestionnaire)
                    .Include(GrievanceDocumentFields.IncludesLetterOfAuth)
                    .Include(GrievanceDocumentFields.IncludesIncomeExpenseForms)
                    .Include(GrievanceDocumentFields.IncludesIncomeExpenseExclusion)
                    .Include(GrievanceDocumentFields.IncludesSupportingDocumentation)
                    .Include(GrievanceDocumentFields.FiveSignatureType)

                    .Include(GrievanceDocumentFields.Complainant)
                    .Include(GrievanceDocumentFields.AttorneyGroup)
                    .Include(GrievanceDocumentFields.AttorneyEmail)
                    .Include(GrievanceDocumentFields.AttorneyPhone)
                    .Include(GrievanceDocumentFields.AttorneyDataRaw)
                    .Include(GrievanceDocumentFields.ComplainantMailAddress)
                    .Include(GrievanceDocumentFields.CoOpUnitNum)
                    .Include(GrievanceDocumentFields.Reason)
                    .Include(GrievanceDocumentFields.Notes)

                    .Include(GrievanceDocumentFields.NysRP525Tentative)
                    .Include(GrievanceDocumentFields.NysRP525IsReduced)
                    .Include(GrievanceDocumentFields.NysRP525IsReducedValue)
                    .Include(GrievanceDocumentFields.NysRP525Answers)

                    .Exclude(GrievanceDocumentFields.InternalDatabaseId);

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
                .Filter.Eq(GrievanceDocumentFields.IsDeleted, true);

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
                    GrievanceDocumentFields.InternalDatabaseId
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
                    .Include(GrievanceDocumentFields.TaxMapId)
                    .Include("email")
                    .Include("submit_date")
                    .Exclude(GrievanceDocumentFields.InternalDatabaseId);

            var taxIdFilter = Builders<BsonDocument>.Filter.Eq(GrievanceDocumentFields.TaxMapId, taxMapId);
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

        public IEnumerable<GrievanceMissingRp524> GetDocumentsByField<T>(
            IMongoCollection<BsonDocument> collection,
            string field1Name,
            IEnumerable<T> values)
        {
            //var projection = 
            //    Builders<BsonDocument>
            //        .Projection
            //        .Include(GrievanceDocumentFields.GuidString)
            //        .Include(GrievanceDocumentFields.TaxMapId)
            //        .Exclude(GrievanceDocumentFields.InternalDatabaseId);

            var filter = Builders<BsonDocument>.Filter.In(field1Name, values);
            var documents = collection.Find(filter).ToCursor().ToEnumerable(); //.Project(projection).ToCursor().ToEnumerable();

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
            var filter = Builders<BsonDocument>.Filter.Eq(GrievanceDocumentFields.GuidString, guid);
            ProjectionDefinition<BsonDocument> projection =
                Builders<BsonDocument>
                    .Projection
                    .Include(GrievanceDocumentFields.TaxMapId)
                    .Exclude(GrievanceDocumentFields.InternalDatabaseId);
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
                { GrievanceDocumentFields.GuidString, submissionGuid },
                { GrievanceDocumentFields.TaxMapId, taxMapId },
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
                { GrievanceDocumentFields.IncludesIncomeExpenseForms, includesIncomeExpenseForms },
                { GrievanceDocumentFields.IncludesIncomeExpenseExclusion, includesIncomeExpenseExclusion },
                { GrievanceDocumentFields.IncludesSupportingDocumentation, includesSupportingDocumentation },
                { "five_signature_type", signatureType },

                { "complainant", complainant },
                { "attorney_group", attorneyGroup },
                { "attorney_phone", attorneyPhone },
                { GrievanceDocumentFields.AttorneyDataRaw, attorneyDataRaw },
                { "complainant_mail_address", complainantMailAddress },
                { "co_op_unit_num", coOpUnitNum },
                { "reason", reason },
                { "notes", notes}
                //{ GrievanceDocumentFields.PaginationId, maxId }
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
                Builders<BsonDocument>.Filter.Eq(GrievanceDocumentFields.GuidString, grievanceId);

            UpdateDefinition<BsonDocument> updateDef =
                new BsonDocument("$set", new BsonDocument(GrievanceDocumentFields.IsDeleted, true));

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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, documentId);

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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, grievanceId);
            
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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, grievanceId);

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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, grievanceId);

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
                        .Set(GrievanceDocumentFields.IncludesIncomeExpenseForms, includesIncomeExpenseForms);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesIncomeExpenseExclusion)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocumentFields.IncludesIncomeExpenseExclusion, includesIncomeExpenseExclusion);
                collection
                    .UpdateOne(filter, update);
            }
            if (includesSupportingDocumentation)
            {
                var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocumentFields.IncludesSupportingDocumentation, includesSupportingDocumentation);
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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, guid);

            // Our Cloud Run containers (based on Alpine) appear to not have timezones installed?
            // var timeUtc = DateTime.UtcNow; 
            // TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            // DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocumentFields.Downloaded, reviewed)
                        .Set(GrievanceDocumentFields.DownloadDate, (reviewed) ? DateTime.UtcNow.ToString() : "")
                        .Set(GrievanceDocumentFields.DownloadDateUnix, (reviewed) ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : -1);

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
                fieldName: GrievanceDocumentFields.GuidString, 
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
            NysRps525OnlineFormAnswers answers
        )
        {
            Contract.Requires(collection != null && answers != null);

            var filter =
                Builders<BsonDocument>
                    .Filter.Eq(GrievanceDocumentFields.GuidString, submissionId);

            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocumentFields.NysRP525Answers, JsonSerializer.Serialize(answers))
                        .Set(GrievanceDocumentFields.NysRP525Tentative, answers.Admin_Rp525_Tentative)
                        .Set(GrievanceDocumentFields.NysRP525IsReduced, answers.Admin_Rp525_Check2a)
                        .Set(GrievanceDocumentFields.NysRP525IsReducedValue, answers.Admin_Rp525_Total)
                        .Set(GrievanceDocumentFields.NysRP525IsNotReduced, answers.Admin_Rp525_Check2b);

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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, guidString);

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
                    .Filter.In(GrievanceDocumentFields.GuidString, grievanceIds);

            // Our Cloud Run containers (based on Alpine) appear to not have timezones installed?
            //var timeUtc = DateTime.UtcNow; 
            //TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var update =
                Builders<BsonDocument>
                    .Update
                        .Set(GrievanceDocumentFields.DispositionEmailSent, setStatusToTrue)
                        .Set(GrievanceDocumentFields.DispositionEmailSentDate, DateTime.UtcNow.ToString());

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
                    .Filter.Eq(GrievanceDocumentFields.GuidString, grievance.guid);
            
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

        private static BsonArray BuildBsonArray<T>(List<T> submissions)
        {
            var submissionsBsonArray = new BsonArray();
            submissions.ForEach((s) => { submissionsBsonArray.Add(s.ToBsonDocument());});
            return submissionsBsonArray;
        }
    }
}
