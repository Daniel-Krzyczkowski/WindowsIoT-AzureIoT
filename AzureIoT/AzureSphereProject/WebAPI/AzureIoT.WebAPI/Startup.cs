using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Resources;
using AzureIoT.WebAPI.Services;
using AzureIoT.WebAPI.Services.Config;
using AzureIoT.WebAPI.Services.DataServices;
using AzureIoT.WebAPI.Services.Interfaces;
using AzureIoT.WebAPI.Services.Mappers;
using AzureIoT.WebAPI.Services.MessagingServices;
using AzureIoT.WebAPI.Services.Settings;
using AzureIoT.WebAPI.Services.StorageServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureIoT.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var azureAdB2CSettings = new AzureAdB2CSettings()
            {
              ClientId =  Configuration["AzureAdB2C:ClientId"],
              Tenant = Configuration["AzureAdB2C:Tenant"],
              Policy = Configuration["AzureAdB2C:Policy"]
            };

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.Authority = $"https://login.microsoftonline.com/tfp/{azureAdB2CSettings.Tenant}/{azureAdB2CSettings.Policy}/v2.0/";
                    jwtOptions.Audience = azureAdB2CSettings.ClientId;
                    jwtOptions.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = AuthenticationFailed
                    };
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var tableStorageSettings = new TableStorageSettings()
            {
                ConnectionString = Configuration["TableStorageConfiguration:ConnectionString"]
            };
            services.AddSingleton(tableStorageSettings);

            var messagingSettings = new IoTHubSettings()
            {
                ServiceClientConnectionString = Configuration["Messaging:ServiceClientConnectionString"]
            };
            services.AddSingleton(messagingSettings);

            services.AddSingleton<ISensorDataService<SensorData>, SensorDataService>();
            services.AddSingleton(typeof(ITableStorageService<TemperatureEntity>), typeof(TemperatureTableStorageService));
            services.AddSingleton(typeof(ITableStorageService<HumidityEntity>), typeof(HumidityTableStorageService));
            services.AddSingleton<IMessagingService, MessagingService>();

            AutoMapperConfiguration.Configure();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }

        private Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            // Should be romved from production code:
            var s = $"AuthenticationFailed: {arg.Exception.Message}";
            arg.Response.ContentLength = s.Length;
            arg.Response.Body.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            return Task.FromResult(0);
        }
    }
}
