// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("K2Bridge.Tests.UnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace K2Bridge
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    /// <summary>
    /// Entry point for the K2 program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Program args.</param>
        public static void Main(string[] args)
        {
            RunAspNetCore(args);
            Log.Logger.Information("***** ALPHA VERSION. MICROSOFT INTERNAL ONLY! *****");
        }

        /// <summary>
        /// Use Asp.Net Core Api platform.
        /// </summary>
        /// <param name="args">commandline arguments.</param>
        private static void RunAspNetCore(string[] args)
        {
            // initialize logger
            // TODO: move logger settings to config.
            Log.Logger = new LoggerConfiguration()

                // .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .UseStartup<Startup>();
        }
    }
}
