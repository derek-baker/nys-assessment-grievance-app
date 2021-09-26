using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Library.Storage;
using Library.Services.Crypto;
using Library.Models;
using Contracts;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Clients.Storage;

namespace Library.Services.Filesystem
{
    public class ZipFileService : IZipFileService
    {
        private readonly UserSettingsRepository _userSettings;

        public ZipFileService(UserSettingsRepository userSettings)
        {
            _userSettings = userSettings;
        }

        /// <summary>
        /// Create archive in-memory
        /// </summary>
        public async Task<byte[]> CreateZip(
            Dictionary<string, FileNameInfo> guidToParcelIdMap,
            Dictionary<string, List<Google.Apis.Storage.v1.Data.Object>> guidToObjectsMap,
            IStorage storageClient,
            StorageSettings settings
        )
        {
            Contract.Requires(
                storageClient != null && guidToObjectsMap != null && settings != null
            );
            var zipOutputStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(zipOutputStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var year = (await _userSettings.GetUserSettings()).Year.ToString();
                foreach (var groupKey in guidToObjectsMap.Keys)
                {
                    foreach (var blobInfo in guidToObjectsMap[groupKey])
                    {
                        // TODO: Refactor this string-parsing out of here.
                        var nameClean = blobInfo.Name.Count(x => x == '/') > 2
                            // WARNING: This assumes there is only one extra unwanted forward-slash
                            ? blobInfo.Name.ReplaceAt(blobInfo.Name.LastIndexOf('/'), '_')
                            : blobInfo.Name;
                        // TODO: Refactor this string-parsing out of here.
                        string[] blobNameParts =
                            nameClean
                                .Replace(year, "")
                                .Replace("//", "")
                                .Split('/');

                        if (blobNameParts.Length != 3) 
                        { 
                            throw new Exception("The variable blobNameParts did not parse as expected. Please investigate."); 
                        }

                        int randomNumber = RandomNumberService.GenerateRandomNumber();
                        var repGroup = clean(
                            string.IsNullOrEmpty(guidToParcelIdMap[groupKey].RepGroup)
                                ? "Pro-Se"
                                : guidToParcelIdMap[groupKey].RepGroup
                        );
                        string zipEntryName =
                            // This is sketchy both for array access and assumption of the file being a PDF
                            $"{repGroup}_{guidToParcelIdMap[groupKey].TaxMapId}/{blobNameParts[2].Replace(".pdf", $"_{randomNumber}.pdf")}";

                        ZipArchiveEntry zipEntry =
                            zipArchive.CreateEntry(
                                zipEntryName
                            );
                        using var zipStream = zipEntry.Open();
                        using MemoryStream fileMemStream = await storageClient.DownloadObject(
                            blobInfo.Name,
                            settings.BucketNameGrievances
                        ).ConfigureAwait(false);
                        fileMemStream.Seek(0, SeekOrigin.Begin);
                        fileMemStream.CopyTo(zipStream);
                    }
                }  
                
                static string clean(string toClean)
                {
                    return toClean
                        .Replace(" ", "_")
                        .Replace(".", "")
                        .Replace("/", "")
                        .Replace(",", "")
                        .Replace("-", "");
                }
            }
            byte[] zipBytes = zipOutputStream.ToArray();
            return zipBytes;
        }
    }
}
