﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Time;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Immutable;
using Library.Services.Image;
using Library.Storage;
using Library.Models;
using Library.Email;
using Library.Services.Filesystem;
using Library.Services.PDF;
using Library.Services.Email;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Clients.Storage;
using Library.Services.Clients.Email;

namespace App.Controllers
{
    /// <summary>
    /// TODO: Refactor filesystem methods to service
    /// </summary>
    [Route("api/[controller]/[action]")]    
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IStorage _storageClient;
        private readonly IEmailClient _email;
        private readonly IImageService _img;
        private readonly StorageSettings _storageSettings;
        private readonly EmailSettings _emailSettings;
        private readonly DocumentDatabaseSettings _dbSettings;
        private readonly UserSettingsRepository _userSettings;
        private readonly GrievanceRepository _grievances;

        public UploadController(
            ILogger<UploadController> logger,
            IStorage storageClient,
            IEmailClient email,
            IImageService img,
            GrievanceRepository grievances,
            UserSettingsRepository userSettings,
            StorageSettings storageSettings,
            EmailSettings emailSettings,
            DocumentDatabaseSettings dbSettings)
        {
            _logger = logger;
            _grievances = grievances;
            _storageClient = storageClient;
            _email = email;
            _img = img;
            _userSettings = userSettings;

            _storageSettings = storageSettings;
            _emailSettings = emailSettings;
            _dbSettings = dbSettings;
        }

        private async Task processSerializedData(
            string grievanceId,
            string taxMapId,
            string formDataSerialized,
            string signatureFour,
            string signatureFive,
            IStorage client
        )
        {
            var formData = JsonSerializer.Deserialize<NysRp524FormData>(formDataSerialized);

            // write RP-524 answers to file in temp
            var formDataFileName = FilenameService.CreateRp524AnswersJsonFilename(grievanceId);
            var year = (await _userSettings.GetUserSettings()).Year.ToString();

            var formDataFilePath = Path.Combine(Path.GetTempPath(), formDataFileName);
            System.IO.File.WriteAllText(
                formDataFilePath,
                JsonSerializer.Serialize(formData)
            );

            // upload JSON file containing RP-524 answers to cloud storage
            await client.UploadToStorageAsync(
                fileBytes: System.IO.File.ReadAllBytes(formDataFilePath),
                currentYear: year,
                submissionGuid: grievanceId,
                fileName: formDataFileName,
                bucketName: _storageSettings.BucketNameGrievances
            );

            var sig4file = $"signature4_{grievanceId}_{TimeService.GetFormattedDate(DateTime.Now)}.jpg";
            var tempFileSignature4Path = Path.Combine(
                Path.GetTempPath(),
                sig4file
            );
            _img.ConvertBase64ToJpg(base64Img: signatureFour, savePath: tempFileSignature4Path);

            var sig5file = $"signature5_{grievanceId}_{TimeService.GetFormattedDate(DateTime.Now)}.jpg";
            var tempFileSignature5Path = Path.Combine(
                Path.GetTempPath(),
                sig5file
            );
            _img.ConvertBase64ToJpg(base64Img: signatureFive, savePath: tempFileSignature5Path);

            // Create copies of NYS PDF (in temp) (NOTE: The '_0' is a hack to ensure a unique filename)
            string readOnlyOutputPdfFilename = $"NYS_RP524_{grievanceId}_{TimeService.GetFormattedDate(DateTime.Now)}_0.pdf";
            string readOnlyOutputPdfFilepath = Path.Combine(
                Path.GetTempPath(),
                readOnlyOutputPdfFilename
            );
            
            string outputPdfFilename = TempFileService.SanitizeFileName($"NYS_RP524_{grievanceId}_{TimeService.GetFormattedDate(DateTime.Now)}.pdf");            
            string outputPdfFilepath = Path.Combine(
                Path.GetTempPath(), 
                outputPdfFilename
            );
            // TODO: This isn't guaranteed to prevent collisions
            System.IO.File.Copy(PdfMergeService.GetPathToFillableNysForm(), readOnlyOutputPdfFilepath);
            System.IO.File.Copy(PdfMergeService.GetPathToFillableNysForm(), outputPdfFilepath);            
            // Fill copy of NYS PDF
            PdfFillService.FillRp524(
                blankPdfToFillPath: readOnlyOutputPdfFilepath, 
                outputPdfPath: outputPdfFilepath, 
                data: formData,
                signature4Path: tempFileSignature4Path,
                signature5Path: tempFileSignature5Path
            );
            var outputFileName = TempFileService.SanitizeFileName(
                $"{grievanceId}_{taxMapId}_{TimeService.GetFormattedDate(DateTime.Now)}.pdf"
            );
            var outputFilePath = Path.Combine(
                Path.GetTempPath(),
                outputFileName
            );
            PdfMergeService.MergeFilledNYSPdfWithNYSBoardOnlyPage(
                outputFilePath,
                outputPdfFilepath
            );
            await client.UploadToStorageAsync(
                fileBytes: System.IO.File.ReadAllBytes(outputFilePath),
                currentYear: year,
                submissionGuid: grievanceId,
                fileName: FilenameService.CreateRp524FileName(formData), // outputPdfFilename,
                bucketName: _storageSettings.BucketNameGrievances
            );

            _ = Task.Run(() => {            
                var filesToDelete = new List<string>() {
                    formDataFilePath,
                    outputFilePath,
                    tempFileSignature4Path,
                    tempFileSignature5Path
                };
                foreach (var filepath in filesToDelete)
                {
                    try
                    {
                        // In GCP Cloud Run, files use memory
                        System.IO.File.Delete(filepath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                    }
                }
            });  
        }


        /// <summary>
        /// Used to create the intitial RP-524 submission     
        /// </summary>
        /// <param name="signatureFour">May or may not be present in form, so we handle that.</param>
        [HttpPost]
        [ActionName("PostInitialSubmission")]
        public async Task<IActionResult> PostInitialSubmission(
            [FromForm] List<IFormFile> files,
            [FromForm] string formData,            
            [FromForm] string inputEmail,
            [FromForm] string taxMapId,
            [FromForm] string signatureFour,
            [FromForm] string signatureFive,
            [FromForm] bool includesPersonalHearing,
            [FromForm] bool includesConflictOfInterest,
            [FromForm] bool includesComQuestionnaire,
            [FromForm] bool includesResQuestionnaire,
            [FromForm] bool includesLetterOfAuthorization,

            [FromForm] bool includesIncomeExpenseForms,
            [FromForm] bool includesIncomeExpenseExclusion,
            [FromForm] bool includesSupportingDocumentation,

            [FromForm] string[] reason            
        )
        {
            var userSettings = await _userSettings.GetUserSettings();

            if (DateTime.Now < userSettings.SubmissionsStartDate || DateTime.Now > userSettings.SubmissionsEndDate) 
                return StatusCode(403);

            string grievanceId = _grievances.GetNewGuid();
            if (
                string.IsNullOrEmpty(formData)
                ||
                string.IsNullOrEmpty(signatureFive)
                ||
                string.IsNullOrEmpty(inputEmail)
                ||
                string.IsNullOrEmpty(grievanceId)
                ||
                string.IsNullOrEmpty(taxMapId)
            )
            {
                throw new ArgumentException("One or more params are invalid");
            }                
            signatureFour = 
                string.IsNullOrEmpty(signatureFour) 
                    ? PdfFillService.FallbackSignatureImage : signatureFour;

            await processSerializedData(
                grievanceId: grievanceId,
                taxMapId: taxMapId,
                formDataSerialized: formData,
                signatureFour,
                signatureFive,
                _storageClient
            );
            var fileNames = new List<string>() { "NYS_RP-524.pdf" };
            // Process supporting docs files.
            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(
                    file.ContentDisposition
                ).FileName.Trim('"');
                var cleanFilename = FilenameService.CreateFilename(fileName);
                fileNames.Add(cleanFilename);

                using var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();
                await _storageClient.UploadToStorageAsync(
                    fileBytes: fileBytes,
                    currentYear: userSettings.Year.ToString(),
                    submissionGuid: grievanceId,
                    fileName: cleanFilename,
                    bucketName: _storageSettings.BucketNameGrievances
                );
            }
            var rp524Answers = JsonSerializer.Deserialize<NysRp524FormData>(formData);
            _grievances.InsertGrievance(
                submissionGuid: grievanceId,
                taxMapId: taxMapId,
                applicantEmail: inputEmail,
                settings: _dbSettings,
                creationMechanism: "Owner/Rep Submission",
                complaintType: rp524Answers.ComputeComplaintType(),
                proposedValue: rp524Answers.MarketValueEstimate,
                isPersonalHearingRequested: includesPersonalHearing,
                includesConflictOfInterest: includesConflictOfInterest,
                includesResQuestionnaire: includesResQuestionnaire,
                includesComQuestionnaire: includesComQuestionnaire,
                includesLetterOfAuth: includesLetterOfAuthorization,
                includesIncomeExpenseForms: includesIncomeExpenseForms,
                includesIncomeExpenseExclusion: includesIncomeExpenseExclusion,
                includesSupportingDocumentation: includesSupportingDocumentation,
                signatureType: string.IsNullOrEmpty(rp524Answers.five_signature_type) ? "Electronically Drawn" : rp524Answers.five_signature_type,
                complainant: rp524Answers.OwnerNameLine1,
                attorneyGroup: rp524Answers.RepInfo,
                attorneyPhone: rp524Answers.RepInfo,
                attorneyDataRaw: rp524Answers.RepInfoComplete,
                complainantMailAddress: $"{rp524Answers.OwnerAddressLine1} {rp524Answers.OwnerAddressLine2}",
                coOpUnitNum: "",
                reason: reason != null ? string.Join(", ", reason) : "Value",
                notes: ""
            );

            var encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetAppUrlFromAmbientInfo(encodedUrl);

            // TODO: Handle error if it occurs. User should know that their submission succeeded, but their confirmation email did not.
            await _email.SendInitialSubmissionEmail(
                to: inputEmail,
                from: _emailSettings.From,
                filenames: $"{JsonSerializer.Serialize(fileNames)}",
                guidString: grievanceId,
                hostForLink: host,
                taxMapId: taxMapId
            );

            var conflicts = _grievances.GetConflictingSubmitters(taxMapId);

            // Deal with conflicting submissions
            if (conflicts.ConflictingApplications.Count > 1)
            {
                var distinctListOfConflictingSubmitters =
                    conflicts.ConflictingApplications.Select(a => a.email).Distinct().ToList();

                var msg = EmailContentGeneratorService.GenerateConflictingSubmissionsHtml(
                    // TODO: Sort these by date?
                    conflicts.ConflictingApplications.Select(
                        a => $"{a.email} - {a.submit_date}"
                    ).ToImmutableList(),
                    taxMapId
                );
                
                _email.SendConflictingSubmissionsEmail(
                    toList: distinctListOfConflictingSubmitters,
                    from: _emailSettings.From,
                    subject: EmailSettings.ConflictingSubmissionsEmailSubject,
                    html: msg);
            }
            return Ok(new { grievanceId });            
        }


        /// <summary>
        /// Used to add supporting docs to an existing submission
        /// </summary>
        [HttpPost]
        [ActionName("PostAppendSupportingDocs")]
        public async Task<IActionResult> PostAppendSupportingDocs(
            [FromForm] List<IFormFile> files,
            [FromForm] string inputEmail,
            [FromForm] string inputGuid,
            [FromForm] string inputTaxMapId,
            [FromForm] bool includesPersonalHearing,
            [FromForm] bool includesConflictOfInterest,
            [FromForm] bool includesComQuestionnaire,
            [FromForm] bool includesResQuestionnaire,
            [FromForm] bool includesLetterOfAuthorization,
            [FromForm] bool includesIncomeExpenseForms,
            [FromForm] bool includesIncomeExpenseExclusion,
            [FromForm] bool includesSupportingDocumentation)
        {
            if (
                string.IsNullOrEmpty(inputEmail)
                ||
                string.IsNullOrEmpty(inputGuid)
                ||
                files == null)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    JsonSerializer.Serialize(new { Msg = "INVALID REQUEST" }));
            }
            inputGuid = inputGuid.Trim();
            if (!_grievances.TestGuidExistence(inputGuid)) 
                return StatusCode(403, JsonSerializer.Serialize(new { Msg = "INVALID GUID"}));            

            var userSettings = await _userSettings.GetUserSettings();

            if (DateTime.Now < userSettings.SubmissionsStartDate) 
            {
                return StatusCode(403, JsonSerializer.Serialize(new { Msg = "SUBMISSIONS NOT STARTED" }));
            }
            if (DateTime.Now > userSettings.SubmissionsEndDate)  // <== Time (EST => UTC) mismatch probably
            {
                return StatusCode(403, JsonSerializer.Serialize(new { Msg = "SUBMISSIONS ENDED" }));
            }            

            // TODO: Task.WhenAll() 
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=netcore-3.1
            if (includesPersonalHearing)
            {
                _grievances.UpdateIsRequestingPersonalHearing(inputGuid, includesPersonalHearing);
            }
            await _grievances.UpdateIncludesDocumentFields(
                inputGuid,
                includesConflictOfInterest,
                includesComQuestionnaire,
                includesResQuestionnaire,
                includesLetterOfAuthorization,
                includesIncomeExpenseForms,
                includesIncomeExpenseExclusion,
                includesSupportingDocumentation);

            var fileNames = new List<string>();
            // Process non-RP524 files (we'd like to assume these are all PDF files...)
            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(
                    file.ContentDisposition
                ).FileName.Trim('"');

                var cleanFilename = FilenameService.CreateFilename(fileName);
                fileNames.Add(cleanFilename);

                using var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();
                await _storageClient.UploadToStorageAsync(
                    fileBytes: fileBytes,
                    currentYear: userSettings.Year.ToString(),
                    submissionGuid: inputGuid,
                    fileName: cleanFilename,
                    bucketName: _storageSettings.BucketNameGrievances
                );
            }

            Uri encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetAppUrlFromAmbientInfo(encodedUrl);

            await _email.SendSupportingDocsEmail(
                to: inputEmail,
                from: _emailSettings.From,
                filenames: $"{JsonSerializer.Serialize(fileNames)}",
                guidString: inputGuid,
                hostForLink: host,
                taxMapId: inputTaxMapId
            );                
            return Ok();            
        }
    }
}
