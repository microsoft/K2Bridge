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
    using K2Bridge.Factories;
    using K2Bridge.KustoDAL;
    using K2Bridge.Models;
    using K2Bridge.RewriteRules;
    using K2Bridge.Telemetry;
    using K2Bridge.Visitors;
    using Kusto.Data.Net.Client;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Prometheus;
    using Serilog;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const string HealthCheckRoute = "/health";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration for the app.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureTelemetryServices(services);

            services.AddControllers();

            services.AddSingleton(Log.Logger);

            services.AddSingleton<IConnectionDetails, KustoConnectionDetails>(
                s => KustoConnectionDetailsFactory.MakeFromConfiguration(Configuration as IConfigurationRoot));

            services.AddSingleton(
                s => MetadataConnectionDetailsFactory.MakeFromConfiguration(Configuration as IConfigurationRoot));

            services.AddSingleton<IQueryExecutor, KustoQueryExecutor>(
                s =>
                {
                    var conn = KustoQueryExecutor
                        .CreateKustoConnectionStringBuilder(s.GetRequiredService<IConnectionDetails>());

                    return new KustoQueryExecutor(
                        KustoClientFactory.CreateCslQueryProvider(conn),
                        KustoClientFactory.CreateCslAdminProvider(conn),
                        s.GetRequiredService<ILogger<KustoQueryExecutor>>(),
                        s.GetRequiredService<Telemetry.Metrics>());
                });

            services.AddHttpContextAccessor();
            services.AddScoped(
                s => new RequestContext(s.GetRequiredService<IHttpContextAccessor>()));

            services.AddTransient<ITranslator, ElasticQueryTranslator>();

            services.AddTransient<IKustoDataAccess, KustoDataAccess>(
                s => new KustoDataAccess(
                    s.GetRequiredService<IQueryExecutor>(),
                    s.GetRequiredService<RequestContext>(),
                    s.GetRequiredService<ILogger<KustoDataAccess>>(),
                    GetConfigOptional<double>("dynamicSamplePercentage"))); // TODO - Add validations

            services.AddTransient<ISchemaRetrieverFactory, SchemaRetrieverFactory>(
                s => new SchemaRetrieverFactory(
                    s.GetRequiredService<ILogger<SchemaRetriever>>(),
                    s.GetRequiredService<IKustoDataAccess>()));

            services.AddTransient<IVisitor, ElasticSearchDSLVisitor>(
                s => new ElasticSearchDSLVisitor(
                    s.GetRequiredService<ISchemaRetrieverFactory>(),
                    s.GetRequiredService<IConnectionDetails>().DefaultDatabaseName));

            services.AddTransient<IResponseParser, KustoResponseParser>(
                s => new KustoResponseParser(
                    s.GetRequiredService<ILogger<KustoResponseParser>>(),
                    GetConfig<bool>("outputBackendQuery"),
                    s.GetRequiredService<Telemetry.Metrics>()));

            // use this http client factory to issue requests to the metadata elastic instance
            services.AddHttpClient(MetadataController.ElasticMetadataClientName, (svcProvider, elasticClient) =>
            {
                var metadataConnectionDetails = svcProvider.GetRequiredService<MetadataConnectionDetails>();
                elasticClient.BaseAddress = new Uri(metadataConnectionDetails.MetadataEndpoint);
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

            app.UseMiddleware<CorrelationIdHeaderMiddleware>();

            // detailed request logging
            app.UseSerilogRequestLogging();

            // Additional rewrite rules to allow different
            // kibana routing behaviors.
            var options = new RewriteOptions()
                .Add(new RewriteRequestsForTemplateRule())
                .Add(new RewriteFieldCapabilitiesRule())
                .Add(new RewriteSearchRule())
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

                // Special treatment for certains requests that are intentionally not marked with the [ApiController] attribute.
                endpoints.MapControllerRoute("fieldcaps", "FieldCapability/Process/{indexName?}", defaults: new { controller = "FieldCapability", action = "Process" });
                endpoints.MapFallbackToController("Passthrough", "Metadata");

                // Enable middleware to serve from health endpoint
                endpoints.MapHealthChecks(HealthCheckRoute);
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
        /// Configures all telemetry services - internal (like promothues endpoint) and external (Application Insights).
        /// </summary>
        private void ConfigureTelemetryServices(IServiceCollection services)
        {
            // using GetService since TelemetryClient won't exist if AppInsights is turned off.
            services.AddSingleton(
                s => Telemetry.Metrics.Create(s.GetService<TelemetryClient>()));

            // complete the config for AppInsights.
            ConfigureApplicationInsights(services);
        }

        /// <summary>
        /// Configures the ApplicationInsights telemetry.
        /// </summary>
        private void ConfigureApplicationInsights(IServiceCollection services)
        {
            // Only if explicitly declared we are collecting telemetry
            var isCollect = GetConfigOptional<bool>("collectTelemetry");

            if (isCollect ?? false)
            {
                return;
            }

            var adxUrl = GetConfig<string>("adxClusterUrl");

            // verify we got a valid instrumentation key, if we didn't, we just skip AppInsights
            // we do not log this, as at this point we still don't have a logger
            var instrumentationKey = GetConfigOptional<Guid>("instrumentationKey");
            if (!instrumentationKey.HasValue)
            {
                return;
            }

            services.AddApplicationInsightsTelemetry(instrumentationKey.ToString());
            var telemetryIdentifier = ComputeSHA256(adxUrl);

            services.AddHttpContextAccessor();
            services.AddSingleton<ITelemetryInitializer>(s =>
                new TelemetryInitializer(s.GetRequiredService<IHttpContextAccessor>(), telemetryIdentifier, HealthCheckRoute));
        }

        private T GetConfig<T>(string key)
        {
            var config = ((IConfigurationRoot)Configuration)[key];
            var type = typeof(T);
            if (type == typeof(string))
            {
                return (T)(object)config;
            }

            if (type == typeof(bool))
            {
                return (T)(object)bool.Parse(config);
            }

            if (type == typeof(double))
            {
                return (T)(object)double.Parse(config);
            }

            if (type == typeof(Guid))
            {
                return (T)(object)Guid.Parse(config);
            }

            throw new InvalidOperationException($"Unsupported type {type}");
        }

        private T? GetConfigOptional<T>(string key)
            where T : struct
        {
            var config = ((IConfigurationRoot)Configuration)[key];
            var type = typeof(T);

            if (type == typeof(bool))
            {
                return bool.TryParse(config, out var result) ? new T?((T)(object)result) : null;
            }

            if (type == typeof(double))
            {
                return double.TryParse(config, out var result) ? new T?((T)(object)result) : null;
            }

            if (type == typeof(Guid))
            {
                return Guid.TryParse(config, out var result) ? new T?((T)(object)result) : null;
            }

            throw new InvalidOperationException($"Unsupported type {type}");
        }
    }
}