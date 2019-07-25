namespace K2Bridge
{
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        private const string ConfigFileName = "appsettings.json";

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(ConfigFileName, false, true)
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
