using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;
using Library.Services;
using Library.Models.DataTransferObjects;
using Library.Models.NoSQLDatabaseSchema;
using Library.Storage;
using Library.Models;
using Library.Services.PDF;
using Library.Services.Filesystem;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Clients.Storage;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly IDocumentDatabase _dbClient;
        private readonly IStorage _storageClient;
        private readonly UserSettingsRepository _userSettings;
        private readonly DocumentDatabaseSettings _dbSettings;
        private readonly StorageSettings _storageSettings;
        private readonly GrievanceRepository _grievances;

        public AdminController(
            IDocumentDatabase dbClient,
            IStorage storage,
            UserSettingsRepository userSettings,
            DocumentDatabaseSettings dbSettings,
            StorageSettings storageSettings)
        {
            _dbClient = dbClient;
            _storageClient = storage; 
            _dbSettings = dbSettings;
            _storageSettings = storageSettings;
            _userSettings = userSettings;
        }

        // GET: /api/<controller>/<action>
        [HttpGet]
        [ActionName("GetGrievanceData")]
        public IActionResult GetGrievanceData()
        {
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);

            var data = _grievances.GetAll();
            return Ok(JsonSerializer.Serialize(data));
        }

        // GET: /api/<controller>/<action>
        [CustomAuth]
        [HttpGet]
        public IActionResult GetChangeReport(DateTime start, DateTime end)
        {
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            var rawPartialData = _grievances.GetChangeList(collection, start, end);

            // TODO: Report class based on fields included from DB
            var partialData = rawPartialData
                .Select(d => BsonSerializer.Deserialize<GrievanceApplication>(d))
                .ToList();

            return Ok(partialData);
        }

        /// <summary>
        /// TODO: The raw grievance JSON is stored in cloud BLOB storage. Move that data to the database.
        /// GET: /api/<controller>/<action>
        /// </summary>
        [CustomAuth]
        [HttpPost]
        [ActionName("GetGrievanceFiles")]
        public async Task<IActionResult> GetGrievanceFiles([FromBody] GetGrievanceFileParams parameters)
        {
            List<Google.Apis.Storage.v1.Data.Object> objects = 
                _storageClient.ListObjectsForSubmission(parameters.SubmissionGuid, _storageSettings.BucketNameGrievances);

            var year = (await _userSettings.GetUserSettings()).Year.ToString();

            return Ok(
                objects.Select(
                    obj => new { 
                        FriendlyName = formatFileName(obj.Name, parameters.SubmissionGuid, year), 
                        FullName = obj.Name 
                    }
                )
            );

            // TODO: Refactor to shared location
            static string formatFileName(string blobName, string guidStr, string year)
            {
                return blobName
                    .Replace(guidStr, "")
                    .Replace(DateTime.Now.Year.ToString(), "")
                    .Replace(year, "")
                    .Replace("//", "");
            }
        }

        [CustomAuth]
        [HttpPost]
        [ActionName("DeleteGrievanceFile")]
        public async Task<IActionResult> DeleteGrievanceFile([FromBody] DeleteFileParams parameters)
        {
            await _storageClient.DeleteObject(
                objectName: parameters.blobFullName, 
                bucketName: _storageSettings.BucketNameGrievances
            );

            return Ok();
        }

        [CustomAuth]
        [HttpGet]
        [ActionName("GetGrievanceJson")]
        public async Task<IActionResult> GetGrievanceJson([FromQuery] string guidString)
        {
            if (string.IsNullOrEmpty(guidString))
                return StatusCode(StatusCodes.Status400BadRequest);
            
            var objects = _storageClient.ListObjectsForSubmission(guidString, _storageSettings.BucketNameGrievances);
                
            var storageObj = objects.Where(
                o => o.Name.Contains(MagicStringsService.Rp524AnswersFileObjectPrefix) && o.Name.EndsWith(".json")
            ).FirstOrDefault();                
            if (storageObj == null)
            {
                // TODO: Handle this with a more meaningful code
                return Ok();
            }
            using MemoryStream inMemoryFile = await _storageClient.DownloadObject(
                storageObj.Name,
                _storageSettings.BucketNameGrievances
            ).ConfigureAwait(false);
            string json = Encoding.UTF8.GetString(inMemoryFile.ToArray()); 
            return Ok(json);
        }

        [CustomAuth]
        [HttpPost]
        [ActionName("PostEditGrievance")]
        public async Task<IActionResult> PostEditGrievance([FromBody] PostGrievanceEditParams reqParams)
        {
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            await _grievances.UpdateGrievance(
                collection,
                reqParams.grievance
            ).ConfigureAwait(false);
            return Ok();
        }

        // POST: api/<controller>/<action>
        [CustomAuth]
        [HttpPost]
        [ActionName("PostDownloadedStatus")]
        public IActionResult PostDownloadedStatus([FromBody] SubmissionUpdateParams parameters)
        {
            if (parameters.guid is null) { throw new Exception("Guid param is null"); }
            var collection = _dbClient.GetCollection(collectionName: _dbSettings.GrievancesCollectionName);
            _grievances.UpdateReviewStatus(collection, parameters.guid, parameters.isReviewed);
            return Ok();            
        }

        // POST: api/<controller>/<action>
        [CustomAuth]
        [HttpPost]
        [ActionName("PostPersonalHearingStatus")]
        public IActionResult PostPersonalHearingStatus([FromBody] GrievanceHearingIsCompletedUpdateParams parameters)
        {
            if (parameters.guid == null) { throw new Exception("Guid param is null"); }
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            _grievances.UpdateCompletedPersonalHearing(collection, parameters.guid, parameters.isHearingCompleted);
            return Ok();
        }

        /// <summary>
        /// POST: api/<controller>/PostAddDocsToSubmission
        /// </summary>        
        [CustomAuth]
        [HttpPost]
        [ActionName("PostAddDocsToSubmission")]
        public async Task<IActionResult> PostAddDocsToSubmission(
            [FromForm] List<IFormFile> files,
            [FromForm] string submissionGuid,
            [FromForm] bool includesPersonalHearing,
            [FromForm] bool includesConflictOfInterest,
            [FromForm] bool includesResQuestionnaire,
            [FromForm] bool includesComQuestionnaire,
            [FromForm] bool includesLetterOfAuthorization,
            [FromForm] bool includesIncomeExpenseForms,
            [FromForm] bool includesIncomeExpenseExclusion,
            [FromForm] bool includesSupportingDocumentation)
        {
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            _grievances.UpdateIncludesFileTypeFields(
                collection,
                submissionGuid,
                includesPersonalHearing,
                includesConflictOfInterest,
                includesResQuestionnaire,
                includesComQuestionnaire,
                includesLetterOfAuthorization,
                includesIncomeExpenseForms,
                includesIncomeExpenseExclusion,
                includesSupportingDocumentation
            );

            var year = (await _userSettings.GetUserSettings()).Year.ToString();

            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(
                    file.ContentDisposition
                ).FileName.Trim('"');
                var cleanFilename = createFilename(fileName);
                    
                using var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();
                await _storageClient.UploadToStorageAsync(
                    fileBytes: fileBytes,
                    currentYear: year,
                    submissionGuid: submissionGuid,
                    fileName: cleanFilename,
                    bucketName: _storageSettings.BucketNameGrievances
                );
            }
            return Ok();

            // TODO: Refactor
            static string createFilename(string fileName)
            {
                // TODO: Something to help ensure filename is unique
                return fileName;
            }
        }
        
        // TODO: This should be in the 
        private async Task tryParseRp524Pdf(
            string newGuid, 
            string fileName, 
            IFormFile file
        )
        {
            if (!fileName.Contains(".pdf")) { return; }
            var filesToDelete = new List<string>();

            // Get temp file path
            string pdfFilePath = Path.Combine(
                Path.GetTempPath(),
                $"PotentialNysRp524_{Guid.NewGuid()}.pdf"
            );

            // Write file to stream
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            byte[] fileBytes = ms.ToArray();
            using var fileStream = new FileStream(pdfFilePath, FileMode.Create);
            fileStream.Write(fileBytes);
            fileStream.Dispose();
            filesToDelete.Add(pdfFilePath);
            
            var parseResult = PdfDataExtractionService.TryParseRp524(pdfFilePath);

            var year = (await _userSettings.GetUserSettings()).Year.ToString();
            if (parseResult.IsParsedSuccessfully)
            {
                // Create json file
                string rp525PrefillJson = JsonSerializer.Serialize(parseResult.ExtractedData);
                string jsonTempFileName = $"Rp525PrefillData_{Guid.NewGuid()}.json";
                string jsonTempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    jsonTempFileName
                );
                System.IO.File.WriteAllText(jsonTempFilePath, rp525PrefillJson);
                filesToDelete.Add(jsonTempFilePath);

                // Upload json to storage with appropriate name
                await _storageClient.UploadToStorageAsync(
                    fileBytes: System.IO.File.ReadAllBytes(jsonTempFilePath),
                    currentYear: year,
                    submissionGuid: newGuid,
                    fileName: FilenameService.CreateRp524AnswersJsonFilename(newGuid),
                    bucketName: _storageSettings.BucketNameGrievances,
                    contentType: "application/json"
                ).ConfigureAwait(false);
            }
            
            // Delete files in list
            foreach (var filepath in filesToDelete)
            {
                try
                {
                    System.IO.File.Delete(filepath);
                }
                catch
                {
                    // TODO: Log the fact that we're leaking memory
                }
            }
        }

        /// <summary>
        /// TODO: Object for params
        /// POST: api/<controller>/PostCreateSubmission
        /// </summary>      
        [CustomAuth]
        [HttpPost]
        [ActionName("PostCreateSubmission")]
        public async Task<IActionResult> PostCreateSubmission(
            [FromForm] List<IFormFile> files,
            [FromForm] string taxMapId,
            [FromForm] string applicantEmail,
            [FromForm] string creatorName,
            [FromForm] string complaintType,
            [FromForm] string proposedValue,
            [FromForm] bool includesPersonalHearing,
            [FromForm] bool includesConflictOfInterest,
            [FromForm] bool includesComQuestionnaire,
            [FromForm] bool includesResQuestionnaire,
            [FromForm] bool includesLetterOfAuthorization,
            [FromForm] bool includesIncomeExpenseForms,
            [FromForm] bool includesIncomeExpenseExclusion,
            [FromForm] bool includesSupportingDocumentation,
            [FromForm] string complainant,
            [FromForm] string attorneyGroup,
            [FromForm] string attorneyPhone,
            [FromForm] string attorneyDataRaw,
            [FromForm] string complainantMailAddress,
            [FromForm] string coOpUnitNum,
            [FromForm] string[] reason,
            [FromForm] string notes
        )
        {
            string newGuid = _grievances.GetNewGuid();
            var year = (await _userSettings.GetUserSettings()).Year.ToString();
            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(
                    file.ContentDisposition
                ).FileName.Trim('"');

                await tryParseRp524Pdf(newGuid, fileName, file).ConfigureAwait(false);

                using var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();

                await _storageClient.UploadToStorageAsync(
                    fileBytes: fileBytes,
                    currentYear: year,
                    submissionGuid: newGuid,
                    fileName: fileName,
                    bucketName: _storageSettings.BucketNameGrievances
                );
            }
            // REMINDER: Do not send confirmation email to submitter here 

            _grievances.InsertGrievance(
                submissionGuid: newGuid,
                taxMapId: taxMapId,
                applicantEmail: applicantEmail,
                settings: _dbSettings,
                creationMechanism: creatorName,
                complaintType: complaintType,
                proposedValue: proposedValue ?? "",
                isPersonalHearingRequested: includesPersonalHearing,
                includesConflictOfInterest: includesConflictOfInterest,
                includesComQuestionnaire: includesComQuestionnaire,
                includesResQuestionnaire: includesResQuestionnaire,
                includesLetterOfAuth: includesLetterOfAuthorization,
                includesIncomeExpenseForms: includesIncomeExpenseForms,
                includesIncomeExpenseExclusion: includesIncomeExpenseExclusion,
                includesSupportingDocumentation: includesSupportingDocumentation,
                signatureType: "Paper",
                complainant: complainant ?? "",
                attorneyGroup: attorneyGroup ?? "",
                attorneyPhone: attorneyPhone ?? "",
                attorneyDataRaw: attorneyDataRaw,
                complainantMailAddress: complainantMailAddress ?? "",
                coOpUnitNum: coOpUnitNum ?? "",
                reason: reason != null ? string.Join(", ", reason) : "Value",
                notes: notes ?? ""
            );
            return Ok();            
        }

        // POST: api/<controller>/PostBarReviewStatus
        [CustomAuth]
        [HttpPost]
        [ActionName("PostBarReviewStatus")]
        public IActionResult PostBarReviewStatus([FromBody] SubmissionUpdateParams parameters)
        {
            if (parameters.guid == null) { throw new Exception("Guid param is null"); }
            var collection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            _dbClient.UpdateBarReviewStatus(collection, parameters.guid, parameters.isReviewed);
            return Ok();            
        }

        [CustomAuth]
        [HttpPost]
        [ActionName("PostGrievanceDeleteRequest")]
        public async Task<IActionResult> PostGrievanceDeleteRequest([FromBody] GrievanceDeletionRequest req)
        {
            await _grievances.DeleteGrievanceSoftly(_dbSettings, req.grievanceId);
            return Ok();
        }

        /// <summary>
        /// POST: api/<controller>/Post525PrefillData
        /// Used by client to allow admin users to download a copy for offline use
        /// </summary>
        [CustomAuth]
        [HttpPost]
        [ActionName("Post525PrefillData")]
        public IActionResult Post525PrefillData([FromBody] RP525FormData rp525Data)
        {
            if (string.IsNullOrEmpty(rp525Data.serializedData)) 
                throw new Exception("A necessary param is null"); 
            
            // Get path to file
            var nysRp525FilePath = PdfFillService.GetPathToBlankNysRp525();
            var tempFilePath = PdfFillService.GetPathForTempNysRp525();
                
            // Create & fill a copy of the RP525 
            PdfFillService.PrefillRp525(
                blankPdfToFillPath: nysRp525FilePath,
                outputPdfPath: tempFilePath,
                data: JsonSerializer.Deserialize<NysRp525PrefillData>(rp525Data.serializedData)
            );
            byte[] filledFileBytes = System.IO.File.ReadAllBytes(tempFilePath);
            System.IO.File.Delete(tempFilePath);

            return File(
                filledFileBytes, 
                // Should this be PDF instead?
                "application/octet-stream"
            );                            
        }

        [HttpGet]
        [ActionName("GetGrievancesMissingRP524")]
        public async Task<IActionResult> GetGrievancesMissingRP524()
        {
            var grievanceCollection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);

            var submissionGuids = await _grievances.GetAllGrievanceIds(grievanceCollection);

            var grievanceIds = _storageClient.FindSubmissionsLackingRp524(
               _storageSettings.BucketNameGrievances,
               (await _userSettings.GetUserSettings())?.Year.ToString(),
               MagicStringsService.NysRp525StorageObjectPrefix,
               "pdf",
               submissionGuids
            );

            var submissionsCollection = _dbClient.GetCollection(_dbSettings.GrievancesCollectionName);
            var data = _grievances.GetDocumentsByField(submissionsCollection, GrievanceDocument.Fields.GuidString, grievanceIds);

            return Ok(data);
        }
    }
}
