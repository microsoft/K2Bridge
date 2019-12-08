// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.RequestHandlers
{
    using System;
    using System.Data;
    using System.Net;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// This class handels the IndexList request to return the available tables in Kusto.
    /// </summary>
    internal class IndexListRequestHandler : RequestHandlerBase
    {
        public IndexListRequestHandler(HttpListenerContext requestContext, IQueryExecutor kustoClient, string requestId, ILogger logger)
            : base(requestContext, kustoClient, requestId, logger)
        {
        }

        public static bool CanAnswer(string rawUrl, string requestString)
        {
            return

                // rawUrl.Contains("_search?ignore_unavailable=true") // 6.8.1
                rawUrl.Contains("_search") // 6.3.2
                && requestString.Contains("indices")
                && requestString.Contains("_index")
            ;
        }

        public string PrepareResponse(string rawUrl)
        {
            try
            {
                string indexName = this.IndexNameFromURL(rawUrl);
                IDataReader kustoResults = this.Kusto.ExecuteControlCommand($".show tables | search TableName: '{indexName}' | project TableName");
                var response = new IndexListResponseElement();

                while (kustoResults.Read())
                {
                    IDataRecord record = kustoResults;
                    var termBucket = TermBucket.Create(record);
                    response.Aggregations.IndexCollection.AddBucket(termBucket);

                    this.Logger.LogDebug($"Found index/table: {termBucket.Key}");
                }

                kustoResults.Close();

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to retrieve indexes");
                throw;
            }
        }
    }
}