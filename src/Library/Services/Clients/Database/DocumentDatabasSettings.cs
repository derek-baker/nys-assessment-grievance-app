using Contracts;

namespace Library.Services.Clients.Database
{
    public class DocumentDatabaseSettings
    {
        public string ConnectionString { get; }
        public string DatabaseName { get; }
        public string GrievancesCollectionName { get; }
        public string DispositionsGenerationQueue { get; }
        public string RepresentativesCollectionName { get; }
        public string UserSettingsCollectionName { get; }
        public string UsersCollectionName { get; }
        public string SessionsCollectionName { get; }

        public DocumentDatabaseSettings(Settings appSettings)
        {
            var user = appSettings.Database.User;
            var pw = appSettings.Database.UserPassword;

            var submissionInfoServer = appSettings.Database.Server;
            DatabaseName = appSettings.Database.DatabaseName;

            ConnectionString = MongoDatabase.BuildConnectionString(
                dbUser: user,
                password: pw,
                server: submissionInfoServer,
                dbName: DatabaseName
            );
            GrievancesCollectionName = appSettings.Database.Collections.Grievances;
            DispositionsGenerationQueue = appSettings.Database.Collections.DispositionQueue;
            UserSettingsCollectionName = appSettings.Database.Collections.Settings;
            RepresentativesCollectionName = appSettings.Database.Collections.Representatives;
            UsersCollectionName = appSettings.Database.Collections.Users;
            SessionsCollectionName = appSettings.Database.Collections.Sessions;
        }
    }
}
