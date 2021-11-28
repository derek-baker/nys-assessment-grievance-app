namespace Contracts
{
    public class Settings
    {
        public string CloudProjectId { get; set; }
        public Database Database { get; set; }
        public Storage Storage { get; set; }
        public Email Email { get; set; }
        public Admin Admin { get; set; }
    }

    public class Database
    {
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string User { get; set; }
        public string UserPassword { get; set; }
        public Collections Collections { get; set; }
    }

    public class Collections
    {
        public string Grievances { get; set; }
        public string DispositionQueue { get; set; }
        public string Representatives { get; set; }
        public string Settings { get; set; }
        public string Users { get; set; }
        public string Sessions { get; set; }
    }

    public class Storage
    {
        public Buckets Buckets { get; set; }
    }

    public class Buckets
    {
        public string Grievances { get; set; }
        public string Dispositions { get; set; }
    }

    public class Email
    {
        public string ApiKey { get; set; }
        public string DefaultFrom { get; set; }
        public string DefaultCc { get; set; }
    }

    public class Admin {
        public string DefaultUser { get; set; }
        public string DefaultPassword { get; set; }
        public int DefaultSecurityCode { get; set; }
    }
}
