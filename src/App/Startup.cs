using App.Extensions;
using Contracts;
using Library.Database;
using Library.Email;
using Library.Models;
using Library.Models.Settings;
using Library.Services._Clients.Secrets;
using Library.Services.Config.UserSettings;
using Library.Services.Csv;
using Library.Services.Filesystem;
using Library.Services.Guid;
using Library.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Library.Services.Auth;
using Library.Services.Image;

namespace App
{
    public class Startup
    {
        const string spaDir = "ClientApp";

        private readonly IConfiguration _configuration;
        private readonly Settings _settings;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var settings = new Settings();
            _configuration.GetSection("Settings").Bind(settings);

            var secrets = new GcpSecretManager(settings.CloudProjectId);
            settings.Database.Server = secrets.GetSecret(SecretKeys.DatabaseServer);
            settings.Database.UserPassword = secrets.GetSecret(SecretKeys.DatabaseUserPassword);
            settings.Email.ApiKey = secrets.GetSecret(SecretKeys.EmailApiKey);
            _settings = settings;            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(
                "ApiPolicy", builder => {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            ));
            services.AddControllers().AddNewtonsoftJson();
            services.AddSpaStaticFiles(
                (configuration) => { configuration.RootPath = $"{spaDir}/dist"; }
            );
            ConfigureDependencyInjection(services, _settings);
        }

        private void ConfigureDependencyInjection(IServiceCollection services, Settings settings)
        {
            services.AddTransient<IDocumentDatabase>(
                s => new MongoDatabase(
                    new DocumentDatabaseSettings(settings).ConnectionString, 
                    settings.Database.DatabaseName)
            );
            services.AddTransient<IStorage, GoogleCloudStorage>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IEmailClient, EmailClient>();
            services.AddTransient<IZipFileService, ZipFileService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IGuidService, GuidService>();
            services.AddTransient<IUserSettingsService, UserSettingsService>();
            services.AddTransient<ICsvGeneratorService, CsvGeneratorService>();

            services.AddSingleton(services => new DocumentDatabaseSettings(settings));
            services.AddSingleton(services => new StorageSettings(settings));
            services.AddSingleton(services => new EmailSettings(settings));
        }

        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env)   
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.ConfigureCustomExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(
                (endpoints) =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}"
                    );
                }
            );
            app.UseSpa(spa => {
                spa.Options.SourcePath = spaDir;
                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
