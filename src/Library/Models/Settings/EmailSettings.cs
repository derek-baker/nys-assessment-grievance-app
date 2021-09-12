namespace Library.Models.Settings
{
    public class EmailSettings
    {
        public string ApiKey { get; }
        public string EmailUsedToLogActivity { get; }
        public string From { get; }

        public static readonly string ConflictingSubmissionsEmailSubject = "Conflicting Assessment Grievance Submissions";

        public EmailSettings(Contracts.Settings settings)
        {
            ApiKey = settings.Email.ApiKey;
            EmailUsedToLogActivity = settings.Email.DefaultCc;
            From = settings.Email.DefaultFrom;
        }

        //public EmailSettings(Dictionary<string, string> config)
        //{
        //    Contract.Requires(config != null);
        //    FaaSUrl = new Uri(config["EmailEndpoint"]);
        //    FaaSApiKey = config["EmailEndpointKey"];
        //}
    }
}
