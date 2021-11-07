using Contracts;
using Library.Email;
using Library.Services.Auth;
using Library.Services.Clients.Database;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Clients.Email;
using Library.Services.Clients.Storage;
using Library.Services.Csv;
using Library.Services.Filesystem;
using Library.Services.Image;
using Library.Storage;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;

namespace App.Config
{
    public static class DependencyInjection
    {
        public static void Configure(IServiceCollection services, Settings settings) 
        {
            services.AddTransient<IDocumentDatabase>(
                s => new MongoDatabase(
                    new DocumentDatabaseSettings(settings).ConnectionString,
                    settings.Database.DatabaseName));

            services.AddTransient<SessionRepository>();
            services.AddTransient<UserRepository>();
            services.AddTransient<UserSettingsRepository>();
            services.AddTransient<RepresentativesRepository>();
            services.AddTransient<GrievanceRepository>();

            services.AddTransient<IStorage, GoogleCloudStorage>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IZipFileService, ZipFileService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<ICsvGeneratorService, CsvGeneratorService>();

            services.AddTransient<ISendGridClient>(s => new SendGridClient(settings.Email.ApiKey));
            services.AddTransient<IEmailClient, EmailClient>();

            services.AddSingleton(services => new DocumentDatabaseSettings(settings));
            services.AddSingleton(services => new StorageSettings(settings));
            services.AddSingleton(services => new EmailSettings(settings));
        }
    }
}
