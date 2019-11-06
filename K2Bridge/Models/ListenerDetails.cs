namespace K2Bridge.Models
{
    using System;
    using Microsoft.Extensions.Configuration;

    internal class ListenerDetails
    {
        private ListenerDetails(string[] prefixes, string remoteEndpoint, bool isCompareResponse, bool isHandleMetadata)
        {
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("URI prefixes are required, for example http://contoso.com:8080/index/");
            }

            if (string.IsNullOrEmpty(remoteEndpoint))
            {
                throw new ArgumentException("URI for remote endpoint is required, for example http://127.0.0.1:8080");
            }

            this.Prefixes = prefixes;
            this.RemoteEndpoint = remoteEndpoint;
            this.IsCompareResponse = isCompareResponse;
            this.IsHandleMetadata = isHandleMetadata;
        }

        public string[] Prefixes { get; private set; }

        public string RemoteEndpoint { get; private set; }

        public bool IsCompareResponse { get; private set; }

        public bool IsHandleMetadata { get; private set; }

        public static ListenerDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new ListenerDetails(
                new string[] { config["bridgeListenerAddress"] },
                config["remoteElasticAddress"],
                bool.Parse(config["isCompareResponse"] ?? "false"),
                bool.Parse(config["isHandleMetadata"] ?? "true"));
    }
}