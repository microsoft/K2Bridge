// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.RequestHandlers
{
    using System.Net;
    using K2Bridge.KustoConnector;
    using Microsoft.Extensions.Logging;

    internal class RequestHandlerBase
    {
        public RequestHandlerBase(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
        {
            this.Logger = logger;

            this.Context = requestContext;

            this.Kusto = kustoClient;

            this.RequestId = requestId;
        }

        protected HttpListenerContext Context { get; private set; }

        protected IQueryExecutor Kusto { get; private set; }

        protected string RequestId { get; private set; }

        protected ILogger Logger { get; private set; }

        protected string IndexNameFromURL(string rawUrl)
        {
            return rawUrl.Substring(1, rawUrl.IndexOf('/', 1) - 1);
        }
    }
}
