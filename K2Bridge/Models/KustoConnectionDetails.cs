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
    internal class KustoConnectionDetails : IConnectionDetails
    {
        private KustoConnectionDetails(
            string clusterUrl,
            string defaultDatabaseName,
            string aadClientId,
            string aadClientSecret,
            string aadTenantId)
        {
            if (string.IsNullOrEmpty(clusterUrl))
            {
                throw new ArgumentException("Kusto Cluster URL is empty");
            }

            if (string.IsNullOrEmpty(defaultDatabaseName))
            {
                throw new ArgumentException("Kusto default database name is empty");
            }

            if (string.IsNullOrEmpty(aadClientId))
            {
                throw new ArgumentException("Kusto AAD Client ID is empty");
            }

            if (string.IsNullOrEmpty(aadClientSecret))
            {
                throw new ArgumentException("Kusto AAD Client Secret is empty");
            }

            if (string.IsNullOrEmpty(aadTenantId))
            {
                throw new ArgumentException("Kusto AAD Tenant ID is emtpy");
            }

            ClusterUrl = clusterUrl;
            DefaultDatabaseName = defaultDatabaseName;
            AadClientId = aadClientId;
            AadClientSecret = aadClientSecret;
            AadTenantId = aadTenantId;
        }

        /// <summary>
        /// Gets and Sets Kusto Cluster URL.
        /// </summary>
        public string ClusterUrl { get; private set; }

        /// <summary>
        /// Gets and Sets Kusto Default Database Name.
        /// </summary>
        public string DefaultDatabaseName { get; private set; }

        /// <summary>
        /// Gets and Sets Kusto AAD Client ID.
        /// </summary>
        public string AadClientId { get; private set; }

        /// <summary>
        /// Gets and Sets Kusto Client Secert.
        /// </summary>
        public string AadClientSecret { get; private set; }

        /// <summary>
        /// Gets and Sets Kusto AAD Tenant ID.
        /// </summary>
        public string AadTenantId { get; private set; }

        /// <summary>
        /// Creates a Kusto Connection Details object using configuration.
        /// </summary>
        /// <param name="config">Configuration element.</param>
        /// <returns>A Kusto Connection Details Object.</returns>
        public static KustoConnectionDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new KustoConnectionDetails(
                config["adxClusterUrl"],
                config["adxDefaultDatabaseName"],
                config["aadClientId"],
                config["aadClientSecret"],
                config["aadTenantId"]);
    }
}