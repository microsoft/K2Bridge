// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
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
        /// Get date histogram aggregate from a given <see cref="DataRowCollection"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="rowCollection">The row collection be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"/>.</returns>
        public static BucketAggregate GetDateHistogramAggregate(string key, DataRowCollection rowCollection, ILogger logger)
        {
            logger.LogTrace("Get date histogram aggregate for {}", key);

            var dateHistogramAggregate = new BucketAggregate();

            foreach (DataRow row in rowCollection)
            {
                var bucket = BucketFactory.CreateDateHistogramBucket(key, row, logger);
                if (bucket != null)
                {
                    dateHistogramAggregate.Buckets.Add(bucket);
                }
            }

            return dateHistogramAggregate;
        }

        /// <summary>
        /// Get range aggregate from a given <see cref="DataRowCollection"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="rowCollection">The row collection be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"/>.</returns>
        public static BucketAggregate GetRangeAggregate(string key, DataRowCollection rowCollection, ILogger logger)
        {
            logger.LogTrace("Get range aggregate for {}", key);

            var rangeAggregate = new BucketAggregate() { Keyed = true };

            foreach (DataRow row in rowCollection)
            {
                var bucket = BucketFactory.CreateRangeBucket(key, row, logger);
                if (bucket != null)
                {
                    rangeAggregate.Buckets.Add(bucket);
                }
            }

            return rangeAggregate;
        }

        /// <summary>
        /// Get date range aggregate from a given <see cref="DataRowCollection"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="rowCollection">The row collection be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="BucketAggregate"></returns>
        public static BucketAggregate GetDateRangeAggregate(string key, DataRowCollection rowCollection, ILogger logger)
        {
            logger.LogTrace("Get date range aggregate for {}", key);

            var rangeAggregate = new BucketAggregate();

            foreach (DataRow row in rowCollection)
            {
                var bucket = BucketFactory.CreateDateRangeBucket(key, row, logger);
                if (bucket != null)
                {
                    rangeAggregate.Buckets.Add(bucket);
                }
            }

            return rangeAggregate;
        }

        /// <summary>
        /// Get terms aggregate from a given <see cref="DataRowCollection"/>.
        /// </summary>
        /// <param name="key">The aggregation key.</param>
        /// <param name="rowCollection">The row collection be parsed.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns><see cref="TermsAggregate"/>.</returns>
        public static TermsAggregate GetTermsAggregate(string key, DataRowCollection rowCollection, ILogger logger)
        {
            logger.LogTrace("Get terms aggregate for {}", key);

            var termsAggregate = new TermsAggregate() { SumOtherDocCount = 0 };

            foreach (DataRow row in rowCollection)
            {
                var bucket = BucketFactory.CreateTermsBucket(key, row, logger);
                if (bucket != null)
                {
                    termsAggregate.Buckets.Add(bucket);
                }
            }

            return termsAggregate;
        }

        /// <summary>
        /// Add aggregates to current <see cref="AggregateDictionary"/> instance from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="aggregateDictionary">AggregateDictionary instance.</param>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be added as aggregate.</param>
        /// <param name="logger">ILogger object for logging.</param>
        public static void AddAggregates(this AggregateDictionary aggregateDictionary, string primaryKey, DataRow row, ILogger logger)
        {
            var columns = row.Table.Columns;

            foreach (DataColumn column in columns)
            {
                if (column.ColumnName == BucketColumnNames.Count || column.ColumnName == primaryKey)
                {
                    continue;
                }

                // Column Metadata (Separator %)
                // Structure: key%metric%value1%value2%keyed
                var columnMetadata = column.ColumnName.Split(AggregationsConstants.MetadataSeparator);

                if (columnMetadata.Length > 1)
                {
                    var metric = columnMetadata[1];

                    if (metric == "percentile")
                    {
                        var key = columnMetadata[0];
                        aggregateDictionary.Add(key, GetPercentileAggregate(column.ColumnName, columnMetadata, row, logger));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to parse column metadata. {metric} is invalid.");
                    }
                }
                else
                {
                    var key = column.ColumnName;
                    aggregateDictionary.Add(key, GetValueAggregate(key, row, logger));
                }
            }
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