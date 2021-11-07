using App.Extensions;
using Contracts;
using Library.Services._Clients.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Library.Services.Clients.Database.Repositories;
using App.Config;

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
            settings.Database.User = secrets.GetSecret(SecretKeys.DatabaseUserName);
            settings.Database.UserPassword = secrets.GetSecret(SecretKeys.DatabaseUserPassword);

            settings.Email.ApiKey = secrets.GetSecret(SecretKeys.EmailApiKey);

            settings.Admin.DefaultUser = secrets.GetSecret(SecretKeys.AppDefaultUserName);
            settings.Admin.DefaultPassword = secrets.GetSecret(SecretKeys.AppDefaultUserPassword);

            _settings = settings;            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(
                "PublicApiOpenCorsPolicy", builder => {
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
            DependencyInjection.Configure(services, _settings);
        }

        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            UserRepository users)   
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
                    spa.UseAngularCliServer(npmScript: "start");                
            });

            var defaultAdminUser = users.GetUser(_settings.Admin.DefaultUser).Result;
            if (defaultAdminUser != null) return;
            users.CreateUser(
                username: _settings.Admin.DefaultUser, 
                password: _settings.Admin.DefaultPassword
            ).Wait();
        }
    }
}
