using Library.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Library.Storage
{
    public interface IStorage
    {
        Task<MemoryStream> DownloadObject(string objectName, string bucketName);

        Task DeleteObject(string objectName, string bucketName);
        List<Google.Apis.Storage.v1.Data.Object> ListObjectsForSubmission(string guidString, string bucketName);
        Task<List<Google.Apis.Storage.v1.Data.Object>> ListObjectsForAllSubmissions(string bucketName);
        Task<Dictionary<string, List<Google.Apis.Storage.v1.Data.Object>>> ListNysRp525ObjectsAssociatedWithEmail(
            string email,
            EmailToGuidLookups guidFilterLookup,
            string rp525FilePrefix,
            string bucketName
        );

        /// <param name="fileBytes"></param>
        /// <param name="currentYear">This is used in a pseudo-folder path</param>
        /// <param name="submissionGuid">This is used in a pseudo-folder path (the final leaf)</param>
        Task<Google.Apis.Storage.v1.Data.Object> UploadToStorageAsync(byte[] fileBytes, string currentYear, string submissionGuid, string fileName, string bucketName, string contentType = "application/octet-stream");


        string BuildUrlForObject(string bucketName, string objectName);

        IEnumerable<string> FindSubmissionsLackingRp524(
            string bucketName,
            string year,
            string filePrefix,
            string fileExtension,
            IEnumerable<string> submissionGuids
        );
    }
}