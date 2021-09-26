namespace Library.Services.Clients.Storage
{
    public class StorageSettings
    {
        public string BucketNameGrievances { get; }
        public string BucketNameDispositions { get; }

        public StorageSettings(Contracts.Settings config)
        {
            BucketNameGrievances = config.Storage.Buckets.Grievances;
            BucketNameDispositions = config.Storage.Buckets.Dispositions;
        }
    }
}
