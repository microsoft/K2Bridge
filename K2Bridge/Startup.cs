// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using K2Bridge.Controllers;
    using K2Bridge.DAL;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.RewriteRules;
    using K2Bridge.Visitors;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Prometheus;
    using Serilog;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Prometheus Histograms to collect query performance data
            var adxQueryDurationMetric = Metrics.CreateHistogram("adx_query_total_seconds", "ADX query total execution time in seconds.", new HistogramConfiguration
            {
                Buckets = Histogram.LinearBuckets(start: 1, width: 1, count: 60),
            });
            var adxNetQueryDurationMetric = Metrics.CreateHistogram("adx_query_net_seconds", "ADX query net execution time in seconds.", new HistogramConfiguration
            {
                Buckets = Histogram.LinearBuckets(start: 1, width: 1, count: 60),
            });
            var adxQueryBytesMetric = Metrics.CreateHistogram("adx_query_result_bytes", "ADX query result payload size in bytes.", new HistogramConfiguration
            {
                Buckets = Histogram.LinearBuckets(start: 1, width: 1, count: 60),
            });

            ConfigureTelemetryServices(services);

            services.AddControllers();

            services.AddSingleton(Log.Logger);

            services.AddSingleton<IConnectionDetails, KustoConnectionDetails>(
                s => KustoConnectionDetails.MakeFromConfiguration(Configuration as IConfigurationRoot));

            services.AddSingleton(
                s => MetadataConnectionDetails.MakeFromConfiguration(Configuration as IConfigurationRoot));

            services.AddSingleton<IQueryExecutor, KustoQueryExecutor>(
                s => new KustoQueryExecutor(
                    s.GetRequiredService<IConnectionDetails>(),
                    s.GetRequiredService<ILogger<KustoQueryExecutor>>(),
                    adxQueryDurationMetric));

            services.AddTransient<ITranslator, ElasticQueryTranslator>();

            services.AddTransient<IVisitor, ElasticSearchDSLVisitor>(
                s => new ElasticSearchDSLVisitor(s.GetRequiredService<IConnectionDetails>().DefaultDatabaseName));

            services.AddTransient<IKustoDataAccess, KustoDataAccess>();

            services.AddTransient<IResponseParser, KustoResponseParser>(
                ctx => new KustoResponseParser(
                    ctx.GetRequiredService<ILogger<KustoResponseParser>>(),
                    bool.Parse((Configuration as IConfigurationRoot)["outputBackendQuery"]),
                    adxNetQueryDurationMetric,
                    adxQueryBytesMetric));

            // use this http client factory to issue requests to the metadata elastic instance
            services.AddHttpClient(MetadataController.ElasticMetadataClientName, (svcProvider, elasticClient) =>
            {
                var metadataConnectionDetails = svcProvider.GetRequiredService<MetadataConnectionDetails>();
                elasticClient.BaseAddress = new Uri(metadataConnectionDetails.MetadataEndpoint);
            });

            services.AddHttpContextAccessor();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "K2Bridge API", Version = "v1" });
            });

            // Add a health/liveness service
            services.AddHealthChecks();

            // required on ASP.NET Core 3 https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio#jsonnet-support
            services.AddMvcCore().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
            }

            // detailed request logging
            app.UseSerilogRequestLogging();

            // Additional rewrite rules to allow different
            // kibana routing behaviors.
            var options = new RewriteOptions()
                .Add(new RewriteRequestsForTemplateRule())
                .Add(new RewriteFieldCapabilitiesRule())
                .Add(new RewriteIndexListRule())
                .Add(new RewriteTrailingSlashesRule());
            app.UseRewriter(options);
            app.UseRouting();

            // Expose HTTP Metrics:
            // Number of HTTP requests in progress.
            // Total number of received HTTP requests.
            // Duration of HTTP requests.
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                // Starts a Prometheus metrics exporter using endpoint routing.
                // Using The default URL: /metrics.
                endpoints.MapMetrics();

                endpoints.MapControllers();

                // Special treatment to FieldCapabilityController as it's intentionally not marked with the [ApiController] attribute.
                endpoints.MapControllerRoute("fieldcaps", "FieldCapability/Process/{indexName?}", defaults: new { controller = "FieldCapability", action = "Process" });
                endpoints.MapControllerRoute("indexlist", "IndexList/Process/{indexName?}", defaults: new { controller = "IndexList", action = "Process" });
                endpoints.MapFallbackToController("Passthrough", "Metadata");

                // Enable middleware to serve from health endpoint
                endpoints.MapHealthChecks("/health");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "K2Bridge API v0.1-alpha");
            });
        }

        /// <summary>
        /// Compute the SHA256 value of a given string.
        /// </summary>
        /// <param name="input">The input string to be hashed.</param>
        /// <returns>A SHA256 hash of the input string.</returns>
        private static string ComputeSHA256(string input)
        {
            var sb = new StringBuilder();
            using (var sha256 = SHA256.Create())
            {
                var result =
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                foreach (var item in result)
                {
                    sb.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Configures the ApplicationInsights telemetry.
        /// </summary>
        private void ConfigureTelemetryServices(IServiceCollection services)
        {
            // Only if explicitly declared we are collecting telemetry
            var hasCollectBool =
                bool.TryParse(
                    (Configuration as IConfigurationRoot)["collectTelemetry"],
                    out bool isCollect);

            if (!hasCollectBool || !isCollect)
            {
                return;
            }

            var adxUrl = (Configuration as IConfigurationRoot)["adxClusterUrl"];

            // verify we got a valid instrumentation key, if we didn't, we just skip AppInsights
            // we do not log this, as at this point we still don't have a logger
            var hasGuid = Guid.TryParse((Configuration as IConfigurationRoot)["instrumentationKey"], out Guid instrumentationKey);
            if (hasGuid)
            {
                services.AddApplicationInsightsTelemetry(instrumentationKey.ToString());
                var telemetryIdentifier = ComputeSHA256(adxUrl);
                services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer(telemetryIdentifier));
            }
        }
    }
}