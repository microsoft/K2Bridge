// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System.Linq;
    using System.Data;
    using System.Globalization;
    using K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using K2Bridge.Models.Response.Aggregations;

    /// <summary>
    /// Aggregate Factory.
    /// </summary>
    internal static class AggregateFactory
    {
        /// <summary>
        /// Add aggregates to current <see cref="AggregateDictionary"> instance from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="aggregateDictionary">AggregateDictionary instance.</param>
        /// <param name="row">The row to be added as aggregate.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <param name="primaryKey">The primary aggregation key.</param>
        public static void AddAggregates(this AggregateDictionary aggregateDictionary, DataRow row, ILogger logger, string primaryKey)
        {
            var columns = row.Table.Columns;

            foreach (DataColumn column in columns)
            {
                if (column.ColumnName == BucketColumnNames.Count || column.ColumnName == primaryKey)
                {
                    continue;
                }

                var columnMetadata = column.ColumnName.Split('%');

                if (columnMetadata.Length > 1)
                {
                    // Column Metadata (Separator %)
                    // Structure: key%metric%value1%value2%keyed
                    var metric = columnMetadata[1];

                    if (metric == "percentile")
                    {
                        var key = columnMetadata[0];
                        aggregateDictionary.Add(key, GetPercentileAggregate(column.ColumnName, columnMetadata, row, logger));
                    }
                }
                else
                {
                    var key = column.ColumnName;
                    logger.LogTrace($"Defining the value for {key}");

                    var rowValue = (double)row[key];
                    double? value = double.IsNaN(rowValue) ? null : rowValue;

                    aggregateDictionary.Add(key, new ValueAggregate() { Value = value });
                }
            }
        }

        /// <summary>
        /// Get <see cref="PercentileAggregate"> object from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnMetadata">The column metadata.</param>
        /// <param name="row">The row to be added as aggregate.</param>
        /// <param name="logger">ILogger object for logging.</param>
        /// <returns></returns>
        private static PercentileAggregate GetPercentileAggregate(string columnName, string[] columnMetadata, DataRow row, ILogger logger)
        {
            // Parse list of percents, and keyed option
            var percents = columnMetadata[2..^1];
            var keyed = bool.Parse(columnMetadata[^1]);

            var percentileAggregate = new PercentileAggregate() { Keyed = keyed };

            if (row[columnName] is JArray percentileValues)
            {
                foreach (var (percent, value) in percents.Zip(percentileValues))
                {
                    percentileAggregate.Values.Add(
                        new PercentileItem()
                        {
                            Percentile = double.Parse(percent, CultureInfo.InvariantCulture),
                            Value = value.Value<double>(),
                        });
                }
            }

            return percentileAggregate;
        }
    }
}