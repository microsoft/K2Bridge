namespace K2Bridge
{
    using System;
    using Microsoft.Extensions.Configuration;

    internal class KustoConnectionDetails
    {
        private KustoConnectionDetails(string kustoClusterUrl, string kustoDatabase, string kustoAadClientId, string kustoAadClientSecret, string kustoAadTenantId)
        {
            if (kustoClusterUrl == null || kustoClusterUrl.Length == 0)
            {
                throw new ArgumentException("Kusto Cluster URL is empty");
            }

            if (kustoDatabase == null || kustoDatabase.Length == 0)
            {
                throw new ArgumentException("Kusto database name is empty");
            }

            if (kustoAadClientId == null || kustoAadClientId.Length == 0)
            {
                throw new ArgumentException("Kusto AAD Client ID is empty");
            }

            if (kustoAadClientSecret == null || kustoAadClientSecret.Length == 0)
            {
                throw new ArgumentException("Kusto AAD Client Secret is empty");
            }

            if (kustoAadTenantId == null || kustoAadTenantId.Length == 0)
            {
                throw new ArgumentException("Kusto AAD Tenant ID is emtpy");
            }

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