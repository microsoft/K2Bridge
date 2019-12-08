// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("K2BridgeUnitTests")]

namespace K2Bridge
{
    using System;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

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
    }
}
