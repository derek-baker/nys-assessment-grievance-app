﻿using App.Services.Mappers;
using Library.Models;
using Library.Services.PDF;
using Library.Storage;
using Microsoft.AspNetCore.Mvc;
using Library.Services;
using Library.Services.Image;
using System;
using System.IO;
using System.Threading.Tasks;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Clients.Storage;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OnlineBarReviewController : ControllerBase
    {
        private readonly IDocumentDatabase _db;
        private readonly IStorage _storage;
        private readonly IImageService _img;
        private readonly UserSettingsRepository _userSettings;
        private readonly DocumentDatabaseSettings _dbSettings;
        private readonly StorageSettings _storageSettings;

        public OnlineBarReviewController(
            IDocumentDatabase db,
            IStorage storage,
            IImageService img,
            UserSettingsRepository userSettings,
            DocumentDatabaseSettings dbSettings,
            StorageSettings storageSettings)
        {
            _db = db;
            _storage = storage;
            _img = img;
            _userSettings = userSettings;

            _dbSettings = dbSettings;
            _storageSettings = storageSettings;
        }

        [CustomAuth]
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
        [CustomAuth]
        [HttpPost]
        [ActionName("PostBarReviewSave")]
        public async Task<IActionResult> PostBarReviewSave(NysRps525OnlineFormData answers)
        {
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
                    answers,
                    isComplete: false
                );
            }
            var answersOnly = PropMapper<NysRps525OnlineFormData, NysRps525OnlineFormAnswers>.From(answers);
            addAnswersToDb(_db, _dbSettings, answersOnly);

            return Ok();
        }

        /// <summary>
        /// POST: api/<controller>/PostBarReviewResult
        /// </summary>
        [CustomAuth]
        [HttpPost]
        [ActionName("PostBarReviewResult")]
        public async Task<IActionResult> PostResult(NysRps525OnlineFormData answers)
        {
            var answersOnly = PropMapper<NysRps525OnlineFormData, NysRps525OnlineFormAnswers>.From(answers);
            addAnswersToDb(_db, _dbSettings, answersOnly);
            var year = (await _userSettings.GetUserSettings()).Year.ToString();
            string tempSignatureFilePath = writeSignatureImageFile(answersOnly, _img);

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
        }

        private static void addAnswersToDb(
            IDocumentDatabase db,
            DocumentDatabaseSettings dbSettings,
            NysRps525OnlineFormAnswers answers)
        {
            var submissionCollection = db.GetCollection(dbSettings.GrievancesCollectionName);
            db.UpdateNysRp525Answers(
                submissionCollection,
                answers.SubmissionGuid,
                answers,
                isComplete: true
            );
        }

        private static PrePdfFillingFileStagingResult stageFilesForPdfFilling(NysRps525OnlineFormAnswers answers)
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
            System.IO.File.Copy(PdfFillService.GetPathToFillableNysRp525Form(), readOnlyOutputPdfFilepath);
            System.IO.File.Copy(PdfFillService.GetPathToFillableNysRp525Form(), outputPdfFilepath);
            return new PrePdfFillingFileStagingResult(readOnlyOutputPdfFilepath, outputPdfFilepath);
        }

        private static string writeSignatureImageFile(NysRps525OnlineFormAnswers answers, IImageService img)
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

        private static string fillNysRp525Pdf(
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