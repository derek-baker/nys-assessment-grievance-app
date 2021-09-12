using Google.Cloud.Storage.V1;
using Library.Models;
using Library.Services.Config.UserSettings;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Storage
{
    public class GoogleCloudStorage : IStorage
    {
        /// <summary>
        /// "We advise you to reuse a single client object if possible"
        /// DOCS: https://googleapis.github.io/google-cloud-dotnet/docs/Google.Cloud.Storage.V1/index.html#client-life-cycle-management
        /// </summary>
        private readonly StorageClient _client = StorageClient.Create();

        private readonly IUserSettingsService _userSettings;

        public GoogleCloudStorage(IUserSettingsService userSettings)
        {
            _userSettings = userSettings;
        }

        //public void CreateBucketIdempontently(
        //    string bucketName,
        //    string projectId
        //)
        //{
        //    var bucket = client.CreateBucket(projectId, bucketName);
        //}

        /// <summary>
        /// TODO: Can we throw if this will overwrite a file?
        /// </summary>        
        public async Task<Google.Apis.Storage.v1.Data.Object> UploadToStorageAsync(
           byte[] fileBytes,
           string currentYear,
           string submissionGuid,
           string fileName,
           string bucketName,
           // TODO: Refactor this param out (it forces us to pollute the interface with implmentation details)
           string contentType = "application/octet-stream"
        )
        {
            Contract.Requires(!string.IsNullOrEmpty(submissionGuid));

            currentYear = (await _userSettings.GetUserSettings()).Year.ToString();

            using var memStream = new MemoryStream(fileBytes);
            var obj = await _client.UploadObjectAsync(
                bucketName,
                $"{currentYear}/{cleanString(submissionGuid)}/{cleanString(fileName)}",
                contentType,
                memStream
            ).ConfigureAwait(false);

            return obj;

            static string cleanString(string toClean)
            {
                Contract.Requires(!string.IsNullOrEmpty(toClean));
                // TODO: Write sanitize method
                return toClean.Trim()
                    .Replace(":", "")
                    .Replace("*", "")
                    .Replace("|", "");
            }
        }

        public List<Google.Apis.Storage.v1.Data.Object> ListObjectsForSubmission(
           string guid,
           string bucketName)
        {
            // See the docs for a more efficient filter when you have larger buckets.
            var bucketObjects = _client.ListObjects(bucketName, "").Where(o => o.Name.Contains(guid));
            return bucketObjects.ToList();
        }

        public IEnumerable<string> FindSubmissionsLackingRp524(
            string bucketName,
            string year,
            string filePrefix,
            string fileExtension,
            IEnumerable<string> submissionGuids
        )
        {
            var rp524ObjectNames = _client
                .ListObjects(bucketName, "")
                .Where(o =>
                    o.Name.Contains(year)
                    &&
                    o.Name.Contains(filePrefix)
                    &&
                    o.Name.Contains(fileExtension)
                )
                .Select(o => o.Name);

            var grievancesMissingRp524 = submissionGuids
                .Where(guid => 
                    rp524ObjectNames.Any(name => !name.Contains(guid))
                );                

            return grievancesMissingRp524;
        }

        public async Task<Dictionary<string, List<Google.Apis.Storage.v1.Data.Object>>> ListNysRp525ObjectsAssociatedWithEmail(
            string email,
            EmailToGuidLookups guidFilterLookups,
            string rp525FilePrefix,
            string bucketName
        )
        {
            Contract.Requires(guidFilterLookups != null);

            // TODO: See the docs for a more efficient filter when you have larger buckets.
            
            List<string> guidsAssociatedWithEmail = guidFilterLookups.Dict[email];
            var guidToObjectsMap = new Dictionary<string, List<Google.Apis.Storage.v1.Data.Object>>();

            var year = (await _userSettings.GetUserSettings()).Year.ToString();
            foreach (var guid in guidsAssociatedWithEmail)
            {
                var rp525ObjectsAssociatedWithGrievance =
                    _client
                        .ListObjects(bucketName, year)
                        .Where(
                            o => o.Name.Contains(guid) && o.Name.Contains(rp525FilePrefix) && !o.Name.EndsWith(".json")
                        );

                var rp525s = rp525ObjectsAssociatedWithGrievance.ToList();
                if (rp525s.ToList().Count > 0)
                {
                    guidToObjectsMap.Add(guid, rp525s.ToList());
                }
            }
            return guidToObjectsMap; 
            //return nysRp525ObjectsAssociatedWithEmail;
        }

        /// <summary>        
        /// TODO: Refactor default vals to config
        /// </summary>        
        public async Task<List<Google.Apis.Storage.v1.Data.Object>> ListObjectsForAllSubmissions(
            string bucketName)
        {
            string fullYear = (await _userSettings.GetUserSettings()).Year.ToString();
            
            // See the docs for a more efficient filter when you have larger buckets.
            // This code assumes we'll be using a pseudo-folder structure with the year as the root folder in the bucket.
            var bucketObjects = 
                _client.ListObjects(bucketName, "")
                    .Where(o => o.Name.Contains(fullYear));

            return bucketObjects.ToList();
        }

        public async Task<MemoryStream> DownloadObject(
            string objectName,
            string bucketName
        )
        {
            var memoryStream = new MemoryStream();
            await _client.DownloadObjectAsync(
                bucketName, objectName, memoryStream
            ).ConfigureAwait(false);
            return memoryStream;
        }

        public async Task DeleteObject(string objectName, string bucketName)
        {
            await _client.DeleteObjectAsync(bucket: bucketName, objectName: objectName);
        }

        /// <summary>
        /// TODO: Return Uri instead of string.
        /// </summary>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:URI-like return values should not be strings", Justification = "<Pending>")]
        public string BuildUrlForObject(string bucketName, string objectName)
        {
            // TODO: Refactor to centralized location
            return "https://storage.googleapis.com/" + bucketName + "/" + objectName;
        }
    }
}
