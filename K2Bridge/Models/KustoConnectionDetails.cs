namespace K2Bridge
{
    using Microsoft.Extensions.Configuration;

    internal class KustoConnectionDetails
    {

        private KustoConnectionDetails(string kustoClusterUrl, string kustoDatabase, string kustoAadClientId, string kustoAadClientSecret, string kustoAadTenantId)
        {
            this.KustoClusterUrl = kustoClusterUrl;
            this.KustoDatabase = kustoDatabase;
            this.KustoAadClientId = kustoAadClientId;
            this.KustoAadClientSecret = kustoAadClientSecret;
            this.KustoAadTenantId = kustoAadTenantId;
        }

        public string KustoClusterUrl { get; private set; }

        public string KustoDatabase { get; private set; }

        public string KustoAadClientId { get; private set; }

        public string KustoAadClientSecret { get; private set; }

        public string KustoAadTenantId { get; private set; }

        public static KustoConnectionDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new KustoConnectionDetails(
                config["kustoClusterUrl"],
                config["kustoDatabase"],
                config["kustoAadClientId"],
                config["kustoAadClientSecret"],
                config["kustoAadTenantId"]);
    }
}