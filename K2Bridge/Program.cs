// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("K2Bridge.Tests.UnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace K2Bridge
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using K2Bridge.Models;
    using K2Bridge.Telemetry;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Serilog;

    /// <summary>
    /// Entry point for the K2 program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly string AssemblyVersion = typeof(Program).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Gets the Configuration for the app.
        /// The config is stored in appsettings.json.
        /// It can also be found on appsettings.Development.json (you local env)
        /// Or in /settings/appsettings.json when deployed to a container.
        /// </summary>
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddJsonFile("settings/appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Program args.</param>
        public static void Main(string[] args)
        {
            // initialize logger
            // Prometheus sink is configured in addition to any sinks defined in appsettings.json.
            var enableQueryLogging = Configuration.GetValue("enableQueryLogging", false);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Sink(new PrometheusSerilogSink())
                .Destructure.ByTransforming<SensitiveData>(obj => new { Data = enableQueryLogging ? obj.Data : obj.RedactMessage })
                .CreateLogger();

            // Log startup message with version as soon as possible
            Log.Logger.Information($"***** Starting K2Bridge {AssemblyVersion} *****");
            Log.Logger.Information($"***** (Supporting Kibana/Elastic v7.16) *****");

            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var bridgeListenerAddress = Configuration["bridgeListenerAddress"];
            if (string.IsNullOrEmpty(bridgeListenerAddress))
            {
                throw new UriFormatException(
                    $"{nameof(bridgeListenerAddress)} is not a valid URI. Please configure the listening address");
            }

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(Configuration)
                .UseUrls(bridgeListenerAddress)
                .UseSerilog()
                .UseStartup<Startup>();
        }
    }
}
