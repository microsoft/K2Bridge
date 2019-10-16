namespace K2Bridge.RequestHandlers
{
    using System.Net;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;

    internal class RequestHandlerBase
    {
        protected HttpListenerContext context;

        protected IQueryExecutor kusto;

        protected string requestId;

        protected ILogger logger;

        public RequestHandlerBase(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
        {
            this.logger = logger;

            this.context = requestContext;

            this.kusto = kustoClient;

            this.requestId = requestId;
        }

        protected string IndexNameFromURL(string rawUrl)
        {
            return rawUrl.Substring(1, rawUrl.IndexOf('/', 1) - 1);
        }
    }
}
