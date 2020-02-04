// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Kusto.Data;
    using Kusto.Data.Data;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Prometheus;

    /// <summary>
    /// Provides parsing methods for kusto response objects.
    /// </summary>
    public class KustoResponseParser : IResponseParser
    {
        private const string AggregationTableName = "aggs";
        private const string HitsTableName = "hits";
        private static readonly Random Random = new Random();
        private static IHistogram kustoNetQueryTime;

        private readonly bool outputBackendQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoResponseParser"/> class.
        /// </summary>
        /// <param name="logger">ILogger object for logging.</param>
        /// <param name="outputBackendQuery">Outputs the backend query during parse.</param>
        /// <param name="adxNetQueryDurationMetric">Prometheus metric to record net query time.</param>
        public KustoResponseParser(ILogger<KustoResponseParser> logger, bool outputBackendQuery, IHistogram adxNetQueryDurationMetric)
        {
            Logger = logger;
            kustoNetQueryTime = adxNetQueryDurationMetric;
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
            Ensure.IsNotNull(kustoResponseDataSet, nameof(kustoResponseDataSet));

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
        public KustoResponseDataSet ReadDataResponse(IDataReader reader)
        {
            var response = KustoDataReaderParser.ParseV1(reader);
            try
            {
                ReportNetQueryExecutionTime(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed logging query net execution time metric.");
            }

            return response;
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
                    return response;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error reading kusto response.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Report net query execution time from Kusto response.
        /// </summary>
        /// <param name="kustoResponseDataSet">Kusto Response.</param>
        private static void ReportNetQueryExecutionTime(KustoResponseDataSet kustoResponseDataSet)
        {
            var queryStatusTable = kustoResponseDataSet[WellKnownDataSet.QueryCompletionInformation];

            Ensure.IsNotNullOrEmpty(queryStatusTable, nameof(queryStatusTable));

            var queryStatusRows = queryStatusTable.First().TableData.Rows;

            if (queryStatusRows.Count <= 1)
            {
                throw new ArgumentException("QueryStatus table missing rows.", nameof(kustoResponseDataSet));
            }

            var statusDescription = queryStatusRows[1]["StatusDescription"];
            Ensure.IsNotNull(statusDescription, nameof(statusDescription));

            var parsedQueryStatus = JObject.Parse(statusDescription.ToString());
            var netQueryExecutionTime = parsedQueryStatus["ExecutionTime"];

            Ensure.IsNotNull(netQueryExecutionTime, nameof(netQueryExecutionTime));

            kustoNetQueryTime.Observe((float)netQueryExecutionTime);
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
                response.AppendBackendQuery(query.QueryCommandText);
            }

            return response;
        }
    }
}