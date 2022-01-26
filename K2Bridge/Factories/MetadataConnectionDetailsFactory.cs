// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using K2Bridge.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// MetadataConnectionDetails Factory.
    /// </summary>
    public static class MetadataConnectionDetailsFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="MetadataConnectionDetails"/>.
        /// </summary>
        /// <param name="config">Config.</param>
        /// <returns>MetadataConnectionDetails.</returns>
        internal static MetadataConnectionDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new (config["metadataElasticAddress"]);
    }
}
