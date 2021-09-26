using App.Services.Mappers;
using Library.Models;
using Library.Models.Settings;
using Library.Services.PDF;
using Library.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using Library.Services;
using Library.Services.Image;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using Library.Models.Entities;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OnlineBarReviewController : ControllerBase
    {
        private readonly IDocumentDatabase _db;
        private readonly IAuthService _authService;
        private readonly IStorage _storage;
        private readonly IImageService _img;
        private readonly UserSettingsRepository _userSettings;
        private readonly DocumentDatabaseSettings _dbSettings;
        private readonly StorageSettings _storageSettings;

        public OnlineBarReviewController(
            IAuthService authService,
            IDocumentDatabase db,
            IStorage storage,
            IImageService img,
            UserSettingsRepository userSettings,
            DocumentDatabaseSettings dbSettings,
            StorageSettings storageSettings)
        {
            _db = db;
            _authService = authService;
            _storage = storage;
            _img = img;
            _userSettings = userSettings;

            _dbSettings = dbSettings;
            _storageSettings = storageSettings;
        }

        [HttpGet]
        [ActionName("GetBarReview")]
        public IActionResult GetBarReview(string submissionGuid)
        {
            var submissionCollection = _db.GetCollection(_dbSettings.GrievancesCollectionName);
            var answers = _db.GetNysRp525Answers(
                submissionCollection,
                submissionGuid
            );            
            return Ok(answers);
        }

        /// <summary>
        /// POST: api/<controller>/PostBarReviewSave
        /// </summary>
        [HttpPost]
        [ActionName("PostBarReviewSave")]
        public async Task<IActionResult> PostBarReviewSave(NysRps525OnlineFormData answers)
        {
            Contract.Requires(answers != null);
            var authResult = await _authService.AuthenticateAndAuthorizeUser(
                answers.UserName,
                answers.Password
            );

            if (!authResult.IsAuthenticated)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            static void addAnswersToDb(
                IDocumentDatabase db,
                DocumentDatabaseSettings dbSettings,
                NysRps525OnlineFormAnswers answers
            )
            {
                // Write seriaized model to db as part of submission document
                var submissionCollection = db.GetCollection(dbSettings.GrievancesCollectionName);
                db.UpdateNysRp525Answers(
                    submissionCollection,
                    answers.SubmissionGuid,
                    answers
                );
            }
            var answersOnly = PropMapper<NysRps525OnlineFormData, NysRps525OnlineFormAnswers>.From(answers);
            addAnswersToDb(_db, _dbSettings, answersOnly);

            return Ok();
        }

        /// <summary>
        /// POST: api/<controller>/PostBarReviewResult
        /// </summary>
        [HttpPost]
        [ActionName("PostBarReviewResult")]
        public async Task<IActionResult> PostResult(NysRps525OnlineFormData answers)
        {
            Contract.Requires(answers != null);
            var authResult = await _authService.AuthenticateAndAuthorizeUser(answers.UserName, answers.Password);
            if (!authResult.IsAuthenticated
                ||
                authResult.Authorization.UserType != AppUserType.AdvancedAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var answersOnly = PropMapper<NysRps525OnlineFormData, NysRps525OnlineFormAnswers>.From(answers);
            addAnswersToDb(_db, _dbSettings, answersOnly);
            var year = (await _userSettings.GetUserSettings()).Year.ToString();

            static string writeSignatureImageFile(NysRps525OnlineFormAnswers answers, IImageService img)
            {
                // write base64 signature image to file
                var signatureFile = $"signature_{answers.SubmissionGuid}_{DateTime.Now:yyyy-M-dd_HH-mm-ss-FFF}.jpg";
                var tempSignatureFilePath = Path.Combine(
                    Path.GetTempPath(),
                    signatureFile
                );
                img.ConvertBase64ToJpg(
                    base64Img: answers.SignatureAsBase64String,
                    savePath: tempSignatureFilePath
                );
                return tempSignatureFilePath;
            }
            string tempSignatureFilePath = writeSignatureImageFile(answersOnly, _img);

            static void addAnswersToDb(
                IDocumentDatabase db,
                DocumentDatabaseSettings dbSettings,
                NysRps525OnlineFormAnswers answers
            )
            {
                // Write serialized model to db as part of submission document
                var submissionCollection = db.GetCollection(dbSettings.GrievancesCollectionName);
                db.UpdateNysRp525Answers(
                    submissionCollection,
                    answers.SubmissionGuid,
                    answers
                );
            }

            static PrePdfFillingFileStagingResult stageFilesForPdfFilling(NysRps525OnlineFormAnswers answers)
            {
                // Create copies of NYS RP-525 PDF (in temp) (NOTE: The '_0' is a hack to ensure a unique filename)
                string readOnlyOutputPdfFilename = 
                    $"{MagicStringsService.NysRp525StorageObjectPrefix}_{answers.SubmissionGuid}_{DateTime.Now:yyyy-M-dd_HH-mm-ss-FFF}_0.pdf";
                string readOnlyOutputPdfFilepath = Path.Combine(
                    Path.GetTempPath(),
                    readOnlyOutputPdfFilename
                );
                string outputPdfFilename = 
                    $"{MagicStringsService.NysRp525StorageObjectPrefix}_{answers.SubmissionGuid}_{DateTime.Now:yyyy-M-dd_HH-mm-ss-FFF}.pdf";
                string outputPdfFilepath = Path.Combine(
                    Path.GetTempPath(),
                    outputPdfFilename
                );
                // TODO: This isn't guaranteed to prevent collisions
                System.IO.File.Copy(PdfFillService.GetPathToFillableNysRp525Form(), readOnlyOutputPdfFilepath);
                System.IO.File.Copy(PdfFillService.GetPathToFillableNysRp525Form(), outputPdfFilepath);
                return new PrePdfFillingFileStagingResult(readOnlyOutputPdfFilepath, outputPdfFilepath);
            }
            PrePdfFillingFileStagingResult filePaths = stageFilesForPdfFilling(answersOnly);

            string pathToFilledPdf = fillNysRp525Pdf(
                filePaths.PathToReadFile, 
                filePaths.PathToWriteFile, 
                answers, 
                tempSignatureFilePath
            );

            // write 525 PDF to cloud storage to include it with other submission documents
            await _storage.UploadToStorageAsync(
                fileBytes: System.IO.File.ReadAllBytes(pathToFilledPdf),
                currentYear: year,
                submissionGuid: answers.SubmissionGuid,
                fileName: $"{MagicStringsService.NysRp525StorageObjectPrefix}_{answers.Admin_Rp525_TaxMapId}.pdf",
                bucketName: _storageSettings.BucketNameGrievances
            );

            // Delete signature file
            System.IO.File.Delete(tempSignatureFilePath);
            // Delete filled PDF
            System.IO.File.Delete(pathToFilledPdf);

            return Ok();

            static string fillNysRp525Pdf(
                string blankPdfToFillPath,
                string outputPdfPath,
                NysRps525OnlineFormData answers,
                string tempSignatureFilePath
            )
            {
                // create filled 525 PDF 
                var outputFilePath = PdfFillService.FillNysRp525(
                    blankPdfToFillPath: blankPdfToFillPath,
                    outputPdfPath: outputPdfPath,
                    data: answers,
                    signaturePath: tempSignatureFilePath
                );
                return outputFilePath;
            }
        }
    }
}