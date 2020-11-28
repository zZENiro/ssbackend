using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SSBackendApp.Cache;
using SSBackendApp.Controllers;
using SSBackendApp.Hubs;
using StackExchange.Redis;

namespace SSBackendApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR();

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_ENDPOINT")));

            services.AddSingleton<IEnumerable<FeaturesCache>>(impl =>
            {
                List<FeaturesCache> features = new List<FeaturesCache>();

                using (var streamReader = System.IO.File.OpenText("Dataset.csv"))
                {
                    var csvReader = new CsvReader(streamReader, CultureInfo.CurrentCulture);

                    csvReader.Configuration.HasHeaderRecord = true;
                    csvReader.Configuration.Delimiter = ",";

                    var _currFeatures = csvReader.GetRecords<FeaturesCache>();

                    foreach (var el in _currFeatures)
                        features.Add(el);
                }

                return features;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestHub>("/hubs/testhub", config =>
                {
                    config.Transports =
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;

                    config.WebSockets.CloseTimeout = TimeSpan.FromSeconds(120);
                    config.LongPolling.PollTimeout = TimeSpan.FromSeconds(30);
                });

                endpoints.MapControllerRoute("default", "{controller}/{action}/{param?}");
            });
        }
    }
}
