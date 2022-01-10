// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    /// <summary>
    /// Holds the different connection details for the kusto cluster. such as database name and credentials.
    /// </summary>
    internal class KustoConnectionDetails : IConnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KustoConnectionDetails"/> class.
        /// </summary>
        /// <param name="clusterUrl"></param>
        /// <param name="defaultDatabaseName"></param>
        /// <param name="aadClientId"></param>
        /// <param name="aadClientSecret"></param>
        /// <param name="aadTenantId"></param>
        internal KustoConnectionDetails(
            string clusterUrl,
            string defaultDatabaseName,
            string aadClientId,
            string aadClientSecret,
            string aadTenantId,
            bool useManagedIdentity = false)
        {
            Ensure.IsNotNullOrEmpty(clusterUrl, "Kusto Cluster URL is empty or null");
            Ensure.IsNotNullOrEmpty(defaultDatabaseName, "Kusto default database name is empty or null");
            Ensure.IsNotNullOrEmpty(aadClientId, "Kusto AAD Client ID is empty or null");
            Ensure.IsNotNullOrEmpty(aadClientSecret, "Kusto AAD Client Secret is empty or null");
            Ensure.IsNotNullOrEmpty(aadTenantId, "Kusto AAD Tenant ID is empty");

            ClusterUrl = clusterUrl;
            DefaultDatabaseName = defaultDatabaseName;
            AadClientId = aadClientId;
            AadClientSecret = aadClientSecret;
            AadTenantId = aadTenantId;
            UseManagedIdentity = useManagedIdentity;
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
        /// Gets a boolean indicating if we need to use a Managed Identity.
        /// </summary>
        public bool UseManagedIdentity { get; private set; }
    }
}