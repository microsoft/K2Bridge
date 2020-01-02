// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Holds the different connection details for the kusto cluster. such as database name and credentials.
    /// </summary>
    internal class KustoConnectionDetails
    {
        private KustoConnectionDetails(
            string kustoClusterUrl,
            string kustoDatabase,
            string kustoAadClientId,
            string kustoAadClientSecret,
            string kustoAadTenantId)
        {
            if (string.IsNullOrEmpty(kustoClusterUrl))
            {
                throw new ArgumentException("Kusto Cluster URL is empty");
            }

            if (string.IsNullOrEmpty(kustoDatabase))
            {
                throw new ArgumentException("Kusto database name is empty");
            }

            if (string.IsNullOrEmpty(kustoAadClientId))
            {
                throw new ArgumentException("Kusto AAD Client ID is empty");
            }

            if (string.IsNullOrEmpty(kustoAadClientSecret))
            {
                throw new ArgumentException("Kusto AAD Client Secret is empty");
            }

            if (string.IsNullOrEmpty(kustoAadTenantId))
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