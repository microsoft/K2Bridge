// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using K2Bridge;
    using K2Bridge.Factories;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.Aggregations;
    using K2Bridge.Telemetry;
    using K2Bridge.Utils;
    using Kusto.Data;
    using Kusto.Data.Data;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides parsing methods for kusto response objects.
    /// </summary>
    public class KustoResponseParser : IResponseParser
    {
        private readonly Metrics metricsHistograms;

        private readonly bool outputBackendQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="KustoResponseParser"/> class.
        /// </summary>
        /// <param name="logger">ILogger object for logging.</param>
        /// <param name="outputBackendQuery">Outputs the backend query during parse.</param>
        /// <param name="metricsHistograms">The instance of the class to record metrics.</param>
        public KustoResponseParser(ILogger<KustoResponseParser> logger, bool outputBackendQuery, Metrics metricsHistograms)
        {
            Logger = logger;
            this.metricsHistograms = metricsHistograms;
            this.outputBackendQuery = outputBackendQuery;
        }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Read Hits from KustoResponseDataSet response.
        /// </summary>
        /// <param name="kustoResponseDataSet">KustoResponseDataSet - Kusto parsed response.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <returns>IEnumerable.<Hit> - collection of hits.</returns>
        public IEnumerable<Hit> ReadHits(KustoResponseDataSet kustoResponseDataSet, QueryData query)
        {
            Ensure.IsNotNull(kustoResponseDataSet, nameof(kustoResponseDataSet));

            if (kustoResponseDataSet[KustoTableNames.Hits] != null)
            {
                using var highlighter = new LuceneHighlighter(query, Logger);
                return HitsMapper.MapRowsToHits(kustoResponseDataSet[KustoTableNames.Hits].TableData.Rows, query, highlighter);
            }

            return Enumerable.Empty<Hit>();
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
                ReportQueryExecutionMetrics(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to log query execution metrics.");
            }

            return response;
        }

        /// <inheritdoc/>
        public ElasticResponse Parse(IDataReader reader, QueryData queryData, TimeSpan timeTaken)
        {
            try
            {
                // Read results and transform to Elastic form
                var response = ReadResponse(queryData, reader, timeTaken);

                // Log total number of hits to have an idea of how many rows were returned by the search expression and aggregated.
                Logger.LogDebug("Total number of hits: {totalHits}", response?.Responses?.First()?.Hits.Total);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error reading kusto response.");
                throw new ParseException("Error reading kusto response", ex);
            }
        }

        /// <summary>
        /// Report net query execution time from Kusto response.
        /// </summary>
        /// <param name="kustoResponseDataSet">Kusto Response.</param>
        private void ReportQueryExecutionMetrics(KustoResponseDataSet kustoResponseDataSet)
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
            var netQueryExecutionTimeValue = (float)netQueryExecutionTime;
            metricsHistograms.AdxNetQueryDurationMetric.Observe(netQueryExecutionTimeValue);
            Logger.LogDebug("[metric] backend query net (engine) duration: {netQueryExecutionTime}", TimeSpan.FromSeconds(netQueryExecutionTimeValue));

            var tableSizeSum = parsedQueryStatus.SelectTokens("dataset_statistics..table_size").Sum(x => x.ToObject<long>());
            if (tableSizeSum > 0)
            {
                metricsHistograms.AdxQueryBytesMetric.Observe(tableSizeSum);
                Logger.LogDebug("[metric] backend query bytes: {tableSizeSum}", tableSizeSum);
            }
            else
            {
                Logger.LogWarning("Backend query bytes is zero.");
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
            var elasticResponse = new ElasticResponse();
            var searchResponse = elasticResponse.Responses.First();

            elasticResponse.AddTook(timeTaken);

            Logger.LogTrace("Reading response using reader.");
            var kustoResponse = ReadDataResponse(reader);

            if (kustoResponse[KustoTableNames.Aggregation] != null)
            {
                var (key, aggregationType) = query.PrimaryAggregation;
                var tableData = kustoResponse[KustoTableNames.Aggregation].TableData;

                // Not all aggregations return a metadata table
                var metadataTableData = kustoResponse[KustoTableNames.Metadata]?.TableData;

                Logger.LogTrace("Parsing aggregations");

                if (string.IsNullOrWhiteSpace(aggregationType))
                {
                    // This is not a bucket aggregation scenario
                    if (tableData.Rows.Count == 0)
                    {
                        tableData.Rows.Add(tableData.NewRow());
                    }

                    foreach (DataRow row in tableData.Rows)
                    {
                        searchResponse.Aggregations.AddAggregates(key, row, query, Logger);
                    }
                }
                else
                {
                    // This a bucket aggregation scenario
                    IAggregate bucketAggregate = aggregationType switch
                    {
                        nameof(Models.Request.Aggregations.DateHistogramAggregation) => AggregateFactory.GetDateHistogramAggregate(key, tableData, query, Logger),
                        nameof(Models.Request.Aggregations.RangeAggregation) => AggregateFactory.GetRangeAggregate(key, tableData, metadataTableData, query, Logger),
                        nameof(Models.Request.Aggregations.DateRangeAggregation) => AggregateFactory.GetDateRangeAggregate(key, tableData, query, Logger),
                        nameof(Models.Request.Aggregations.TermsAggregation) => AggregateFactory.GetTermsAggregate(key, tableData, query, Logger),
                        nameof(Models.Request.Aggregations.FiltersAggregation) => AggregateFactory.GetFiltersAggregate(key, tableData, metadataTableData, query, Logger),
                        nameof(Models.Request.Aggregations.HistogramAggregation) => AggregateFactory.GetHistogramAggregate(key, tableData, query, Logger),
                        _ => null,
                    };

                    searchResponse.Aggregations.Add(key, bucketAggregate);
                }
            }

            // For Range aggregations, the calculated total hits is wrong, so we have an additional column with the expected count
            if (kustoResponse[KustoTableNames.HitsTotal] != null)
            {
                // A single row with a single column
                elasticResponse.SetTotal((long)kustoResponse[KustoTableNames.HitsTotal].TableData.Rows[0][0]);
            }

            // Read hits
            Logger.LogDebug("Reading Hits using QueryData: {@query}", query.ToSensitiveData());
            var hits = ReadHits(kustoResponse, query);
            elasticResponse.AddHits(hits);
            if (outputBackendQuery)
            {
                elasticResponse.AppendBackendQuery(query.QueryCommandText);
            }

            return elasticResponse;
        }
    }
}
