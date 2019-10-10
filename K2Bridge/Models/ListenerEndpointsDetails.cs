namespace K2Bridge.Models
{
    using System;
    using Microsoft.Extensions.Configuration;

    internal class ListenerEndpointsDetails
    {
        private ListenerEndpointsDetails(string[] prefixes, string remoteEndpoint)
        {
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("URI prefixes are required, for example http://contoso.com:8080/index/");
            }

            if (remoteEndpoint == null || remoteEndpoint.Length == 0)
            {
                throw new ArgumentException("URI for remote endpoint is required, for example http://127.0.0.1:8080");
            }

            this.Prefixes = prefixes;
            this.RemoteEndpoint = remoteEndpoint;
        }

        public string[] Prefixes { get; private set; }

        public string RemoteEndpoint { get; private set; }

        public static ListenerEndpointsDetails MakeFromConfiguration(IConfigurationRoot config) =>
            new ListenerEndpointsDetails(new string[] { config["bridgeListenerAddress"] }, config["remoteElasticAddress"]);
    }
}