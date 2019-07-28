namespace K2Bridge
{
    using Microsoft.Extensions.Configuration;
    using System;

    public class Program
    {
        private const string ConfigFileName = "appsettings.json";
        private const string LocalConfigFileName = "appsettings.local.json";

        public static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile(ConfigFileName, false, true)
                .AddJsonFile(LocalConfigFileName, true, true) // Optional for local development
                .Build();

            Serilog.ILogger logger = Logger.GetLogger();

            var translator = new QueryTranslator();

            var kustoManager = new KustoConnector.KustoManager(config);

            SimpleListener simpleListener = new SimpleListener()
            {
                Prefixes = new string[] { config["bridgeListenerAddress"] },
                RemoteEndpoint = config["remoteElasticAddress"],
                Translator = translator,
                Logger = logger,
                KustoManager = kustoManager,
            };

            simpleListener.Start();
        }
    }
}
