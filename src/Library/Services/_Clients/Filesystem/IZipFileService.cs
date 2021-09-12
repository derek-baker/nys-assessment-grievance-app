using Google.Apis.Storage.v1.Data;
using Library.Models;
using Library.Models.Settings;
using Library.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Services.Filesystem
{
    public interface IZipFileService
    {
        Task<byte[]> CreateZip(
            Dictionary<string, FileNameInfo> guidToParcelIdMap, 
            Dictionary<string, List<Object>> objects, 
            IStorage storageClient, 
            StorageSettings settings);
    }
}