// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using System.Linq;

    /// <summary>
    /// Provides parsing methods for kusto response objects
    /// </summary>
    public class KustoResponseParser : IResponseParser
    {
        private readonly ILogger<KustoResponseParser> logger;

        public KustoResponseParser(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<KustoResponseParser>();
        }

        /// <summary>
        /// Parse kusto IDataReader response into ElasticResponse.
        /// </summary>
        /// <param name="reader">Kusto IDataReader response.</param>
        /// <param name="queryData">QueryData containing query information.</param>
        /// <param name="timeTaken">TimeSpan representing query execution duration.</param>
        /// <returns>"ElasticResponse"</returns>
        public ElasticResponse ParseElasticResponse(IDataReader reader, QueryData queryData, TimeSpan timeTaken)
        {
            using (reader)
            {
                try
                {
                    // Read results and transform to Elastic form
                    var response = ReadResponse(queryData, reader, timeTaken);
                    this.logger.LogDebug($"Aggs processed: {response.GetAllAggregations().Count()}");
                    this.logger.LogDebug($"Hits processed: {response.GetAllHits().Count()}");
                    return response;
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Error reading kusto response: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Use the kusto data reader and build an Elastic response.
        /// </summary>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="reader">Kusto IDataReader response.</param>
        /// <param name="timeTaken">TimeSpan representing query execution duration.</param>
        /// <returns>ElasticResponse object.</returns>
        private static ElasticResponse ReadResponse(
            QueryData query,
            IDataReader reader,
            TimeSpan timeTaken)
        {
            var response = new ElasticResponse();

            response.AddTook(timeTaken);
            int tableOrdinal = 0;

            do
            {
                switch (tableOrdinal)
                {
                    case 0:
                        foreach (var agg in reader.ReadAggs())
                        {
                            response.AddAggregation(agg);
                        }

                        break;
                    case 1:
                        foreach (var hit in reader.ReadHits(query))
                        {
                            response.AddHit(hit);
                        }

                        break;
                    default:
                        break;
                }

                tableOrdinal++;
            }
            while (reader.NextResult());
            return response;
        }
    }
}
