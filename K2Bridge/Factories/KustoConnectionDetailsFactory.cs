// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using K2Bridge.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// KustoConnectionDetails Factory.
    /// </summary>
    public static class KustoConnectionDetailsFactory
    {
        /// <summary>
        /// Creates a Kusto Connection Details object using configuration.
        /// </summary>
        /// <param name="config">Configuration element.</param>
        /// <returns>A Kusto Connection Details Object.</returns>
        internal static KustoConnectionDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new KustoConnectionDetails(
                config["adxClusterUrl"],
                config["adxDefaultDatabaseName"],
                config["aadClientId"],
                config["aadClientSecret"],
                config["aadTenantId"]);
    }
}
