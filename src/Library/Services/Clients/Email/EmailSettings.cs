namespace Library.Services.Clients.Email
{
    public class EmailSettings
    {
        public string ApiKey { get; }
        //public string EmailUsedToLogActivity { get; }
        public string From { get; }

        public static readonly string ConflictingSubmissionsEmailSubject = "Conflicting Assessment Grievance Submissions";

        public EmailSettings(Contracts.Settings settings)
        {
            ApiKey = settings.Email.ApiKey;
            //EmailUsedToLogActivity = settings.Email.DefaultCc;
            From = settings.Email.DefaultFrom;
        }
    }
}
