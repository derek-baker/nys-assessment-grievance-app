using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Library.Database;
using Library.Models;
using Library.Models.Settings;
using Library.Services.Config.UserSettings;
using Library.Services.Csv;
using Library.Services.Filesystem;
using Library.Services.PDF;
using Library.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Library.Services;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly StorageSettings _storageSettings;
        private readonly DocumentDatabaseSettings _dbSettings;
        
        private readonly IStorage _storageClient;
        private readonly IDocumentDatabase _db;
        private readonly IUserSettingsService _userSettings;
        private readonly ICsvGeneratorService _csv;

        public DownloadController(
            IStorage storageClient,
            IUserSettingsService userSettings,
            ICsvGeneratorService csv,
            IDocumentDatabase db,
            StorageSettings storageSettings,
            DocumentDatabaseSettings documentDatabaseSettings)
        {
            _storageSettings = storageSettings;
            _dbSettings = documentDatabaseSettings;

            _storageClient = storageClient;
            _userSettings = userSettings;
            _csv = csv;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var fillableRp524FilePath = FilenameService.GetPathToFillableRp524();
            var fillableRp524Bytes = await System.IO.File.ReadAllBytesAsync(
                fillableRp524FilePath
            ).ConfigureAwait(false);

            return File(fillableRp524Bytes, "application/pdf");
        }

        /// <summary>
        /// GET: api/Download/{guidStr}
        /// Used to download .ZIP of all files associated with grievance.
        /// </summary>        
        [HttpGet("{guidStr}", Name = "Get")]
        public async Task<IActionResult> Get(string guidStr)
        {
            // TODO: Refactor to use service method
            var zipOutputStream = new MemoryStream();
            List<Google.Apis.Storage.v1.Data.Object> objects = _storageClient.ListObjectsForSubmission(
                guidStr,
                _storageSettings.BucketNameGrievances
            );
            // We're going to create an archive file.
            using (var zipArchive = new ZipArchive(zipOutputStream, ZipArchiveMode.Create, true))
            {
                var year = (await _userSettings.GetUserSettings())?.Year.ToString();
                foreach (var blobInfo in objects)
                {
                    string zipEntryName =
                        blobInfo.Name
                            .Replace(guidStr, "")
                            .Replace(DateTime.Now.Year.ToString(), "")
                            .Replace(year, "")
                            .Replace("//", "")
                            .Replace("__", "_")
                            .Replace(" ", "_");

                    ZipArchiveEntry zipEntry = 
                        zipArchive.CreateEntry(
                            zipEntryName
                        );
                    using var zipStream = zipEntry.Open();
                    using MemoryStream fileMemStream = await _storageClient.DownloadObject(
                        blobInfo.Name,
                        _storageSettings.BucketNameGrievances
                    ).ConfigureAwait(false);
                    fileMemStream.Seek(0, SeekOrigin.Begin);
                    fileMemStream.CopyTo(zipStream);
                }
            }
            var zipBytes = zipOutputStream.ToArray();
            return File(zipBytes, "application/octet-stream");            
        }

        [HttpGet("ExportGrievancesCsv")]
        public async Task<IActionResult> ExportGrievancesCsv()
        {
            var grievanceCollection = _db.GetCollection(_dbSettings.GrievancesCollectionName);
            var grievances = _db.GetAllGrievances(grievanceCollection);

            var csvBytes = await _csv.Generate(grievances);
            return File(csvBytes, "text/csv");
        }

        /// <summary>
        /// POST: api/Download
        /// CORS is enabled to allow IMO request a prefilled NYS RP-525 
        /// </summary>   
        [EnableCors("ApiPolicy")]
        [HttpPost]
        public IActionResult Post(
            [FromForm] string Muni,
            [FromForm] string OwnerNameLine1,
            [FromForm] string OwnerNameLine2,
            [FromForm] string OwnerAddressLine1,
            [FromForm] string OwnerAddressLine2,
            [FromForm] string OwnerAddressLine3,
            [FromForm] string TaxMapNum,
            [FromForm] string LocationStreetAddress,
            [FromForm] string LocationVillage,
            [FromForm] string LocationCityTown,
            [FromForm] string LocationCounty,
            [FromForm] string TotalVal
        )
        {
            var rp525Data = new NysRp525PrefillData()
            {
                Muni = !string.IsNullOrEmpty(Muni) ? Muni : "",
                OwnerNameLine1 = !string.IsNullOrEmpty(OwnerNameLine1) ? OwnerNameLine1 : "",
                OwnerNameLine2 = !string.IsNullOrEmpty(OwnerNameLine2) ? OwnerNameLine2 : "",
                OwnerAddressLine1 = !string.IsNullOrEmpty(OwnerAddressLine1) ? OwnerAddressLine1 : "",
                OwnerAddressLine2 = !string.IsNullOrEmpty(OwnerAddressLine2) ? OwnerAddressLine2 : "",
                OwnerAddressLine3 = !string.IsNullOrEmpty(OwnerAddressLine3) ? OwnerAddressLine3 : "",
                TaxMapNum = !string.IsNullOrEmpty(TaxMapNum) ? TaxMapNum : "",
                LocationStreetAddress = !string.IsNullOrEmpty(LocationStreetAddress) ? LocationStreetAddress : "",
                LocationVillage = !string.IsNullOrEmpty(LocationVillage) ? LocationVillage : "",
                LocationCityTown = !string.IsNullOrEmpty(LocationCityTown) ? LocationCityTown : "",
                LocationCounty = !string.IsNullOrEmpty(LocationCounty) ? LocationCounty : "",
                TotalVal = !string.IsNullOrEmpty(TotalVal) ? TotalVal : ""
            };            
            var nysRp525FilePath = PdfFillService.GetPathToBlankNysRp525();
            var tempFilePath = PdfFillService.GetPathForTempNysRp525();
            PdfFillService.PrefillRp525(
                blankPdfToFillPath: nysRp525FilePath,
                outputPdfPath: tempFilePath,
                data: rp525Data
            );
            byte[] filledPdfBytes = System.IO.File.ReadAllBytes(tempFilePath);
            System.IO.File.Delete(tempFilePath);
            return File(
                fileDownloadName: $"{MagicStringsService.NysRp525StorageObjectPrefix}_{TaxMapNum}.pdf",
                fileContents: filledPdfBytes,
                contentType: "application/pdf"
            );            
        }
    }    
}
