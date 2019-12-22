// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("K2Bridge.Tests.UnitTests")]

namespace K2Bridge
{
    using System;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Visitors;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    /// <summary>
    /// Entry point for the K2 program
    /// </summary>
    public class Program
    {
        private const string ConfigFileName = "appsettings.json";
        private const string LocalConfigFileName = "appsettings.local.json";

        public static void Main(string[] args)
        {
            // Version notice
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("***** ALPHA VERSION. MICROSOFT INTERNAL ONLY! *****");
            Console.ResetColor();

            // initialize configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(ConfigFileName, false, true)
                .AddJsonFile(LocalConfigFileName, true, true) // Optional for local development
                .AddEnvironmentVariables()
                .Build();

            // todo: remove the listener option:
            // https://dev.azure.com/csedevil/K2-bridge-internal/_sprints/taskboard/K2-bridge-internal%20Team/K2-bridge-internal/Sprint%2002?workitem=1346
            var useAspNet = config.GetValue<bool>("useAspNetCore", false);
            if (useAspNet)
            {
                RunAspNetCore(args);
            }
            else
            {
                RunSimpleListener(config);
            }
        }

        /// <summary>
        /// Use the default simple listener to run the application
        /// </summary>
        private static void RunSimpleListener(IConfigurationRoot config)
        {
            // initialize logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            // initialize DI container
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                      loggingBuilder.AddSerilog(dispose: true))
                .AddScoped(s => KustoConnectionDetails.MakeFromConfiguration(config))
                .AddScoped(s => ListenerDetails.MakeFromConfiguration(config))
                .AddTransient<ITranslator, QueryTranslator>()
                .AddSingleton<IQueryExecutor, KustoManager>()
                .AddTransient<IVisitor, ElasticSearchDSLVisitor>()
                .AddTransient<SimpleListener>()
                .BuildServiceProvider();

            // start service
            serviceProvider.GetService<SimpleListener>().Start();
        }

        /// <summary>
        /// Use Asp.Net Core Api platform.
        /// </summary>
        /// <param name="args">commandline arguments.</param>
        private static void RunAspNetCore(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .UseSerilog();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
