﻿using Library.Database;

namespace Library.Models
{
    public class DocumentDatabaseSettings
    {
        public string ConnectionString { get; }
        public string DatabaseName { get; }
        public string GrievancesCollectionName { get; }
        public string DispositionsGenerationQueue { get; }
        public string UserSettingsCollectionName { get; }
        public string RepresentativesCollectionName { get; }

        public DocumentDatabaseSettings(Contracts.Settings appSettings)
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
        }
    }
}
