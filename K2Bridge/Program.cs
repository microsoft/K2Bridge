namespace K2Bridge
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class Program
    {
        public static void Main(string[] args)
        {
            string proxyAddress = @"http://localhost:8080/";
            //string elasticAddress = @"http://localhost:9200";
            string elasticAddress = @"http://tk8elastic.westeurope.cloudapp.azure.com:9200";

            Serilog.ILogger logger = Logger.GetLogger();

            var translator = new QueryTranslator();

            SimpleListener simpleListener = new SimpleListener()
            {
                Prefixes = new string[] { proxyAddress },
                RemoteEndpoint = elasticAddress,
                Translator = translator,
                Logger = logger,
            };

            simpleListener.Start();
        }
    }
}
