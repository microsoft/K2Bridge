// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models
{
    using System;
    using Microsoft.Extensions.Configuration;

    internal class ListenerDetails
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prefixes"></param>
        /// <param name="metadataEndpoint">URI for metadata Elasticsearch endpoint</param>
        /// <param name="isHandleMetadata"></param>
        private ListenerDetails(string[] prefixes, string metadataEndpoint, bool isHandleMetadata)
        {
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("URI prefixes are required, for example http://contoso.com:8080/index/");
            }

            if (string.IsNullOrEmpty(metadataEndpoint))
            {
                throw new ArgumentException("URI for metadata Elasticsearch endpoint is required, for example http://127.0.0.1:8080");
            }

            this.Prefixes = prefixes;
            this.MetadataEndpoint = metadataEndpoint;
            this.IsHandleMetadata = isHandleMetadata;
        }

        public string[] Prefixes { get; private set; }

        public string MetadataEndpoint { get; private set; }

        public bool IsHandleMetadata { get; private set; }

        public static ListenerDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new ListenerDetails(
                new string[] { config["bridgeListenerAddress"] },
                config["metadataElasticAddress"],
                bool.Parse(config["isHandleMetadata"] ?? "true"));
    }
}
