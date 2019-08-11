namespace K2Bridge
{
    using Microsoft.Extensions.Configuration;
    internal class KustoConnectionDetails
    {
        public string KustoClusterUrl {get; private set;}
        public string KustoDatabase { get; private set;}
        public string KustoAadClientId{ get; private set;}
        public string KustoAadClientSecret{ get; private set;}
        public string KustoAadTenantId {get; private set;}

        private KustoConnectionDetails(string kustoClusterUrl, string kustoDatabase, 
            string kustoAadClientId, string kustoAadClientSecret, string kustoAadTenantId)
        {
            KustoClusterUrl = kustoClusterUrl;
            KustoDatabase = kustoDatabase;
            KustoAadClientId = kustoAadClientId;
            KustoAadClientSecret = kustoAadClientSecret;
            KustoAadTenantId = kustoAadTenantId;
        }

        public static KustoConnectionDetails MakeFromConfiguration(IConfigurationRoot  config) =>
            new KustoConnectionDetails(
                config["kustoClusterUrl"], 
                config["kustoDatabase"],
                config["kustoAadClientId"], 
                config["kustoAadClientSecret"], 
                config["kustoAadTenantId"]);
    }
}