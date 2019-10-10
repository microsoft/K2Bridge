namespace K2Bridge.RequestHandlers
{
    using System;
    using System.Net;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;

    internal class RequestHandler
    {
        protected HttpListenerContext context;

        protected IQueryExecutor kusto;

        protected string requestId;

        protected ILogger logger { get; set; }

        public RequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
        {
            this.logger = logger;

            this.context = requestContext;

            this.kusto = kustoClient;

            this.requestId = requestId;
        }
    }
}
