// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("K2Bridge.Tests.UnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace K2Bridge
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    /// <summary>
    /// Entry point for the K2 program.
    /// </summary>
    public static class Program
    {
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
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(Configuration)
                .UseSerilog()
                .UseStartup<Startup>();
        }
    }
}
