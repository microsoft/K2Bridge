// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.Models.Response;
    using Kusto.Data.Data;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides parsing methods for kusto response objects.
    /// </summary>
    public class KustoResponseParser : IResponseParser
    {
        private const string AggregationTableName = "aggs";
        private const string HitsTableName = "hits";
        private static readonly Random Random = new Random();
        private readonly bool outputBackendQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoResponseParser"/> class.
        /// </summary>
        /// <param name="logger">ILogger object for logging.</param>
        /// <param name="outputBackendQuery">Outputs the backend query during parse.</param>
        public KustoResponseParser(ILogger<KustoResponseParser> logger, bool outputBackendQuery)
        {
            Logger = logger;
            this.outputBackendQuery = outputBackendQuery;
        }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Read Hits from KustoResponseDataSet response.
        /// </summary>
        /// <param name="kustoResponseDataSet">KustoResponseDataSet - Kusto parsed response.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <returns>IEnumerable.<Hit> - collection of hits.</returns>
        public static IEnumerable<Hit> ReadHits(KustoResponseDataSet kustoResponseDataSet, QueryData query)
        {
            if (kustoResponseDataSet[HitsTableName] != null)
            {
                foreach (DataRow row in kustoResponseDataSet[HitsTableName].TableData.Rows)
                {
                    var hit = Hit.Create(row, query);
                    hit.Id = Random.Next().ToString();
                    yield return hit;
                }
            }
        }

        /// <summary>
        /// Parse IDataReader into KustoResponseDataSet response.
        /// </summary>
        /// <param name="reader">Kusto IDataReader response.</param>
        /// <returns>KustoResponseDataSet - parsed response.</returns>
        public static KustoResponseDataSet ReadDataResponse(IDataReader reader)
        {
            return KustoDataReaderParser.ParseV1(reader);
        }

        /// <summary>
        /// Parse kusto IDataReader response into ElasticResponse.
        /// </summary>
        /// <param name="reader">Kusto IDataReader response.</param>
        /// <param name="queryData">QueryData containing query information.</param>
        /// <param name="timeTaken">TimeSpan representing query execution duration.</param>
        /// <returns>"ElasticResponse".</returns>
        public ElasticResponse ParseElasticResponse(IDataReader reader, QueryData queryData, TimeSpan timeTaken)
        {
            // TODO: remove the using statement as the Dispose and Close should be responsibility of the caller.
            using (reader)
            {
                try
                {
                    // Read results and transform to Elastic form
                    var response = ReadResponse(queryData, reader, timeTaken);
                    Logger.LogDebug("Response: {@response}", response);
                    return response;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Error reading kusto response: {ex.Message}");
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
        private ElasticResponse ReadResponse(
            QueryData query,
            IDataReader reader,
            TimeSpan timeTaken)
        {
            var response = new ElasticResponse();

            response.AddTook(timeTaken);

            Logger.LogDebug("Reading response using reader.");
            var parsedKustoResponse = ReadDataResponse(reader);

            if (parsedKustoResponse[AggregationTableName] != null)
            {
                Logger.LogDebug("Parsing aggregations");

                // read aggregations
                foreach (DataRow row in parsedKustoResponse[AggregationTableName].TableData.Rows)
                {
                    var bucket = BucketFactory.MakeBucket(row);
                    response.AddAggregation(bucket);
                }
            }

            // read hits
            Logger.LogDebug("Reading Hits using QueryData: {@query}", query);
            var hits = ReadHits(parsedKustoResponse, query);
            response.AddHits(hits);
            if (outputBackendQuery)
            {
                response.AppendBackendQuery(query.KQL);
            }

            return response;
        }
    }
}
