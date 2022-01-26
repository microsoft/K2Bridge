// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using K2Bridge.Models.Response.Aggregations;
    using K2Bridge.Utils;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Aggregate Factory.
    /// </summary>
    internal static class AggregateFactory
    {
        /// <summary>
        ///  Gets pseudo-random number generator.
        /// </summary>
        private static readonly Random Random = new();

        /// <summary>
        /// Get date histogram aggregate from a given <see cref="DataTable"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"/>.</returns>
        public static BucketAggregate GetDateHistogramAggregate(string key, DataTable dataTable, QueryData query, ILogger logger)
        {
            logger.LogTrace("Get date histogram aggregate for {}", key);

            var dateHistogramAggregate = new BucketAggregate();

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateDateHistogramBucket(key, row, query, logger);
                if (bucket != null)
                {
                    dateHistogramAggregate.Buckets.Add(bucket);
                }
            }

            return dateHistogramAggregate;
        }

        /// <summary>
        /// Get range aggregate from a given <see cref="DataTable"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"/>.</returns>
        public static BucketAggregate GetRangeAggregate(string key, DataTable dataTable, DataTable metadataTable, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(metadataTable, nameof(DataTable));

            logger.LogTrace("Get range aggregate for {}", key);

            var rangeAggregate = new BucketAggregate() { Keyed = true };

            var outputBuckets = new HashSet<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateRangeBucket(key, row, query, logger);
                if (bucket != null)
                {
                    rangeAggregate.Buckets.Add(bucket);
                    outputBuckets.Add(Convert.ToString(row[key]));
                }
            }

            // Get expected bucket names from metadata table
            var expectedBuckets = GetExpectedBuckets(key, metadataTable);

            // Remove bucket names already returned
            expectedBuckets.ExceptWith(outputBuckets);

            // Add missing buckets
            foreach (var missingBucket in expectedBuckets)
            {
                // Add a fake bucket
                var fakeRow = dataTable.NewRow();
                fakeRow[key] = missingBucket;
                var fb = BucketFactory.CreateRangeBucket(key, fakeRow, query, logger);
                rangeAggregate.Buckets.Add(fb);
            }

            return rangeAggregate;
        }

        /// <summary>
        /// Get date range aggregate from a given <see cref="DataTable"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"></returns>
        public static BucketAggregate GetDateRangeAggregate(string key, DataTable dataTable, QueryData query, ILogger logger)
        {
            logger.LogTrace("Get date range aggregate for {}", key);

            var rangeAggregate = new BucketAggregate();

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateDateRangeBucket(key, row, query, logger);
                if (bucket != null)
                {
                    rangeAggregate.Buckets.Add(bucket);
                }
            }

            return rangeAggregate;
        }

        /// <summary>
        /// Get terms aggregate from a given <see cref="DataTable"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="TermsAggregate"/>.</returns>
        public static TermsAggregate GetTermsAggregate(string key, DataTable dataTable, QueryData query, ILogger logger)
        {
            logger.LogTrace("Get terms aggregate for {}", key);

            var termsAggregate = new TermsAggregate() { SumOtherDocCount = 0 };

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateTermsBucket(key, row, query, logger);
                if (bucket != null)
                {
                    termsAggregate.Buckets.Add(bucket);
                }
            }

            return termsAggregate;
        }

        /// <summary>
        /// Get filters aggregate from a given <see cref="DataTable"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"></returns>
        public static BucketAggregate GetFiltersAggregate(string key, DataTable dataTable, DataTable metadataTable, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(metadataTable, nameof(DataTable));

            logger.LogTrace("Get filters aggregate for {}", key);

            var filtersAggregate = new BucketAggregate()
            {
                Keyed = true,
            };

            var outputBuckets = new HashSet<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateFiltersBucket(key, row, query, logger);
                if (bucket != null)
                {
                    filtersAggregate.Buckets.Add(bucket);
                    outputBuckets.Add((string)bucket.Key);
                }
            }

            // Get expected bucket names from metadata table
            var expectedBuckets = GetExpectedBuckets(key, metadataTable);

            // Remove bucket names already returned
            expectedBuckets.ExceptWith(outputBuckets);

            // Add missing buckets
            foreach (var missingBucket in expectedBuckets)
            {
                // Add a fake bucket
                var fakeRow = dataTable.NewRow();
                fakeRow[key] = missingBucket;
                var fb = BucketFactory.CreateFiltersBucket(key, fakeRow, query, logger);
                filtersAggregate.Buckets.Add(fb);
            }

            return filtersAggregate;
        }

        /// <summary>
        /// Gets the expected bucket names for a given key from the metadata table.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="metadataTable">The metadata table.</param>
        /// <returns>A HashSet<string> of expected bucket names.</returns>
        public static HashSet<string> GetExpectedBuckets(string key, DataTable metadataTable)
        {
            var expectedBuckets = new HashSet<string>();

            var reader = metadataTable.CreateDataReader();
            while (reader.Read())
            {
                if (reader.GetString(KustoTableNames.MetadataKey) == key)
                {
                    expectedBuckets.Add(reader.GetString(KustoTableNames.MetadataValue));
                }
            }

            return expectedBuckets;
        }

        /// <summary>
        /// Get histogram aggregate from a given <see cref="DataRowCollection"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="dataTable">The row collection be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"></returns>
        public static BucketAggregate GetHistogramAggregate(string key, DataTable dataTable, QueryData query, ILogger logger)
        {
            logger.LogTrace("Get histogram aggregate for {}", key);

            var histogramAggregate = new BucketAggregate();

            foreach (DataRow row in dataTable.Rows)
            {
                var bucket = BucketFactory.CreateHistogramBucket(key, row, query, logger);
                if (bucket != null)
                {
                    histogramAggregate.Buckets.Add(bucket);
                }
            }

            return histogramAggregate;
        }

        /// <summary>
        /// Add aggregates to current <see cref="AggregateDictionary"> instance from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="aggregateDictionary">AggregateDictionary instance.</param>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be added as aggregate.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">ILogger object for logging.</param>
        public static void AddAggregates(this AggregateDictionary aggregateDictionary, string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            var columns = row.Table.Columns;

            foreach (DataColumn column in columns)
            {
                if (column.ColumnName == BucketColumnNames.Count || column.ColumnName == primaryKey)
                {
                    continue;
                }

                // Column Metadata (Separator %)
                var columnMetadata = column.ColumnName.Split(AggregationsConstants.MetadataSeparator);
                string key;

                if (columnMetadata.Length <= 1)
                {
                    // ColumnName contains only the key (this is a value aggregate)
                    key = column.ColumnName;
                    aggregateDictionary.Add(key, GetValueAggregate(key, row, logger));
                    continue;
                }

                // ColumnName contains key + metadata (metric used to select aggregate type)
                key = columnMetadata[0];
                var metric = columnMetadata[1];

                IAggregate aggregate = metric switch
                {
                    AggregationsConstants.Percentile => GetPercentileAggregate(column.ColumnName, columnMetadata, row, logger),
                    AggregationsConstants.ExtendedStats => GetExtendedStatsAggregate(column.ColumnName, columnMetadata, row, logger),
                    AggregationsConstants.TopHits => GetTopHitsAggregate(column.ColumnName, columnMetadata, row, logger, query),
                    _ => throw new InvalidOperationException($"Failed to parse column metadata. {metric} is invalid."),
                };

                aggregateDictionary.Add(key, aggregate);
            }
        }

        /// <summary>
        /// Get extended stats for standard deviation aggregate from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnMetadata">The column metadata parsed.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="ExtendedStatsAggregate"></returns>
        private static ExtendedStatsAggregate GetExtendedStatsAggregate(string columnName, string[] columnMetadata, DataRow row, ILogger logger)
        {
            logger.LogTrace("Get extended stats for standard deviation aggregate for {}", columnName);

            var extendedStatsAggregate = new ExtendedStatsAggregate();

            if (row[columnName] == DBNull.Value)
            {
                return extendedStatsAggregate;
            }

            var sigma = Convert.ToInt32(columnMetadata[2]);

            var jObject = (JObject)row[columnName];

            var count = jObject[AggregationsConstants.Count]?.Value<long?>();
            extendedStatsAggregate.Count = count.HasValue && !double.IsNaN(count.Value) ? count : null;

            var min = jObject[AggregationsConstants.Min]?.Value<double?>();
            extendedStatsAggregate.Min = min.HasValue && !double.IsNaN(min.Value) ? min : null;

            var max = jObject[AggregationsConstants.Max]?.Value<double?>();
            extendedStatsAggregate.Max = max.HasValue && !double.IsNaN(max.Value) ? max : null;

            var avg = jObject[AggregationsConstants.Average]?.Value<double?>();
            extendedStatsAggregate.Average = avg.HasValue && !double.IsNaN(avg.Value) ? avg : null;

            var sum = jObject[AggregationsConstants.Sum]?.Value<double?>();
            extendedStatsAggregate.Sum = sum.HasValue && !double.IsNaN(sum.Value) ? sum : null;

            var sumOfSquares = jObject[AggregationsConstants.SumOfSquares]?.Value<double?>();
            extendedStatsAggregate.SumOfSquares = sumOfSquares.HasValue && !double.IsNaN(sumOfSquares.Value) ? sumOfSquares : null;

            var variancePopulation = jObject[AggregationsConstants.VariancePopulation]?.Value<double?>();
            extendedStatsAggregate.VariancePopulation = variancePopulation.HasValue && !double.IsNaN(variancePopulation.Value) ? variancePopulation : null;
            extendedStatsAggregate.Variance = extendedStatsAggregate.VariancePopulation;

            var varianceSampling = jObject[AggregationsConstants.VarianceSampling]?.Value<double?>();
            extendedStatsAggregate.VarianceSampling = varianceSampling.HasValue && !double.IsNaN(varianceSampling.Value) ? varianceSampling : null;

            var stdDeviationPopulation = jObject[AggregationsConstants.StandardDeviationPopulation]?.Value<double?>();
            extendedStatsAggregate.StandardDeviationPopulation = stdDeviationPopulation.HasValue && !double.IsNaN(stdDeviationPopulation.Value) ? stdDeviationPopulation : null;
            extendedStatsAggregate.StandardDeviation = extendedStatsAggregate.StandardDeviationPopulation;

            var stdDeviationSampling = jObject[AggregationsConstants.StandardDeviationSampling]?.Value<double?>();
            extendedStatsAggregate.StandardDeviationSampling = stdDeviationSampling.HasValue && !double.IsNaN(stdDeviationSampling.Value) ? stdDeviationSampling : null;

            if (extendedStatsAggregate.StandardDeviationPopulation.HasValue && extendedStatsAggregate.Average.HasValue)
            {
                extendedStatsAggregate.StandardDeviationBounds.LowerPopulation = extendedStatsAggregate.Average.Value - (extendedStatsAggregate.StandardDeviationPopulation.Value * sigma);
            }

            extendedStatsAggregate.StandardDeviationBounds.Lower = extendedStatsAggregate.StandardDeviationBounds.LowerPopulation;

            if (extendedStatsAggregate.StandardDeviationSampling.HasValue && extendedStatsAggregate.Average.HasValue)
            {
                extendedStatsAggregate.StandardDeviationBounds.LowerSampling = extendedStatsAggregate.Average.Value - (extendedStatsAggregate.StandardDeviationSampling.Value * sigma);
            }

            if (extendedStatsAggregate.StandardDeviationPopulation.HasValue && extendedStatsAggregate.Average.HasValue)
            {
                extendedStatsAggregate.StandardDeviationBounds.UpperPopulation = extendedStatsAggregate.Average + (extendedStatsAggregate.StandardDeviationPopulation.Value * sigma);
            }

            extendedStatsAggregate.StandardDeviationBounds.Upper = extendedStatsAggregate.StandardDeviationBounds.UpperPopulation;

            if (extendedStatsAggregate.StandardDeviationSampling.HasValue && extendedStatsAggregate.Average.HasValue)
            {
                extendedStatsAggregate.StandardDeviationBounds.UpperSampling = extendedStatsAggregate.Average + (extendedStatsAggregate.StandardDeviationSampling.Value * sigma);
            }

            return extendedStatsAggregate;
        }

        /// <summary>
        /// Get percentile aggregate from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnMetadata">The column metadata.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="PercentileAggregate"/>.</returns>
        private static PercentileAggregate GetPercentileAggregate(string columnName, string[] columnMetadata, DataRow row, ILogger logger)
        {
            logger.LogTrace("Get percentile aggregate for {}", columnName);

            // Parse list of percents, and keyed option
            var percents = columnMetadata[2..^1];
            var keyed = bool.Parse(columnMetadata[^1]);

            var percentileAggregate = new PercentileAggregate() { Keyed = keyed };

            if (row[columnName] is JArray percentileValues && percentileValues.HasValues)
            {
                foreach (var (percent, value) in percents.Zip(percentileValues))
                {
                    var percentileItem = value.Type switch
                    {
                        // If token type is a string, we assume this is is date time
                        JTokenType.String => new PercentileItem()
                        {
                            Percentile = double.Parse(percent, CultureInfo.InvariantCulture),
                            Value = TimeUtils.ToEpochMilliseconds(value.Value<DateTime>()),
                            ValueAsString = value.Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                        },
                        _ => new PercentileItem()
                        {
                            Percentile = double.Parse(percent, CultureInfo.InvariantCulture),
                            Value = value.Value<double>(),
                        },
                    };

                    percentileAggregate.Values.Add(percentileItem);
                }
            }
            else
            {
                // If row[columnName] is empty, it returns null value for each percentile requested
                foreach (var percent in percents)
                {
                    var percentileItem = new PercentileItem()
                    {
                        Percentile = double.Parse(percent, CultureInfo.InvariantCulture),
                        Value = null,
                    };

                    percentileAggregate.Values.Add(percentileItem);
                }
            }

            logger.LogTrace("Percentile aggregate returned for {}: {}", columnName, percentileAggregate);

            return percentileAggregate;
        }

        /// <summary>
        /// Get tophits aggregate from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnMetadata">The column metadata.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <returns><see cref="TopHitsAggregate"/>.</returns>
        private static TopHitsAggregate GetTopHitsAggregate(string columnName, string[] columnMetadata, DataRow row, ILogger logger, QueryData query)
        {
            logger.LogTrace("Get TopHits aggregate for {}", columnName);

            var topHitsAggregate = new TopHitsAggregate() { };

            topHitsAggregate.Hits.SetTotal(0);

            var rowValue = row[columnName];
            if (rowValue.GetType() != typeof(System.DBNull))
            {
                var docCount = (long)row[BucketColumnNames.Count];
                topHitsAggregate.Hits.SetTotal(docCount);

                if (row[columnName] is JArray { HasValues: true } topHitsValues)
                {
                    var hits = new List<Hit>();

                    foreach (JObject jObject in topHitsValues)
                    {
                        var hit = HitsFactory.Create(Random.Next().ToString(), query.IndexName);

                        var sourceField = jObject[AggregationsConstants.SourceField].Value<string>();
                        var sourceValue = jObject[AggregationsConstants.SourceValue];

                        var sortValue = jObject[AggregationsConstants.SortValue];

                        // Add fields
                        hit.Fields.Add(sourceField, new List<object>() { sourceValue });

                        // Add source
                        hit.AddSource(sourceField, sourceValue);

                        // Add sort (with date conversion to timestamp)
                        if (sortValue.Type == JTokenType.String)
                        {
                            hit.Sort.Add(TimeUtils.ToEpochMilliseconds(sortValue.Value<DateTime>()));
                        }
                        else
                        {
                            hit.Sort.Add(sortValue.Value<double>());
                        }

                        hits.Add(hit);
                    }

                    topHitsAggregate.Hits.AddHits(hits);
                }
            }

            logger.LogTrace("TopHits aggregate returned for {}: {}", columnName, topHitsAggregate);

            return topHitsAggregate;
        }

        /// <summary>
        /// Get value aggregate from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="ValueAggregate"/>.</returns>
        private static ValueAggregate GetValueAggregate(string key, DataRow row, ILogger logger)
        {
            logger.LogTrace("Get value aggregate for {}", key);

            var valueAggregate = new ValueAggregate() { Value = null };
            var rowValue = row[key];

            if (rowValue.GetType() != typeof(System.DBNull))
            {
                valueAggregate = rowValue switch
                {
                    System.DateTime dateValue => new ValueAggregate() { Value = TimeUtils.ToEpochMilliseconds(dateValue), ValueAsString = dateValue.ToString("yyyy-MM-ddTHH:mm:ss.fffK") },
                    _ => new ValueAggregate() { Value = Convert.ToDouble(rowValue) },
                };
            }

            logger.LogTrace("Value aggregate returned for {}: {}", key, valueAggregate);

            return valueAggregate;
        }
    }
}