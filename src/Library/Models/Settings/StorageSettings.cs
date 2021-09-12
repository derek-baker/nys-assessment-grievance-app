using System.Diagnostics.Contracts;

namespace Library.Models.Settings
{
    public class StorageSettings
    {
        public string BucketNameGrievances { get; }
        public string BucketNameDispositions { get; }

        //public StorageSettings(IConfiguration config)
        //{
        //    Contract.Requires(config != null);

        //    BucketName = config["StorageBucketName"];
        //    BucketNamePublic = config["StorageBucketNameDispositions"];
        //}

        public StorageSettings(Contracts.Settings config)
        {
            BucketNameGrievances = config.Storage.Buckets.Grievances;
            BucketNameDispositions = config.Storage.Buckets.Dispositions;
        }

        //public StorageSettings(Dictionary<string, string> config)
        //{
        //    Contract.Requires(config != null);

        //    BucketName = config["StorageBucketName"];
        //    BucketNamePublic = config["StorageBucketNameDispositions"];
        //}
    }
}
