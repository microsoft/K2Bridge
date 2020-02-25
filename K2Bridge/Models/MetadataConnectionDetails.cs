// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    internal class MetadataConnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataConnectionDetails"/> class.
        /// </summary>
        /// <param name="metadataEndpoint">URI for metadata Elasticsearch endpoint.</param>
        internal MetadataConnectionDetails(string metadataEndpoint)
        {
            Ensure.IsNotNullOrEmpty(
                metadataEndpoint,
                nameof(metadataEndpoint),
                "URI for metadata ElasticSearch endpoint is required, for example http://127.0.0.1:8080");

            MetadataEndpoint = metadataEndpoint;
        }

        public string MetadataEndpoint { get; private set; }
    }
}
