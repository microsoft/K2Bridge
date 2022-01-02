// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// BucketFactory.
    /// </summary>
    internal static class BucketFactory
    {
        /// <summary>
        /// Create a new <see cref="DateHistogramBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new DateHistogramBucket.</returns>
        public static DateHistogramBucket CreateDateHistogramBucketFromDataRow(DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            // TODO: timestamp is always the first column (probably named 2), and count will be named _count
            // we currently mix index and column names, need to check if we can enhance this logic
            // workitem 15050
            var timestamp = row[(int)BucketColumnNames.SummarizeByColumn];
            var count = row[BucketColumnNames.Count];
            var dateBucket = (DateTime)timestamp;

            var dhb = new DateHistogramBucket {
                DocCount = Convert.ToInt32(count),
                Key = TimeUtils.ToEpochMilliseconds(dateBucket),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                Aggs = new Dictionary<string, Dictionary<string, object>>(),
            };

            CreateAggregationColumns(dhb, row, logger);

            return dhb;
        }

        /// <summary>
        /// Create a new <see cref="Bucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new Bucket.</returns>
        public static Bucket CreateTermsBucketFromDataRow(DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var key = row[(int)BucketColumnNames.SummarizeByColumn];
            var count = row[BucketColumnNames.Count];

            var tb = new Bucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToString(key),
                Aggs = new Dictionary<string, Dictionary<string, object>>(),
            };

            CreateAggregationColumns(tb, row, logger);

            return tb;
        }

        /// <summary>
        /// Create a new <see cref="RangeBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new TermsBucket.</returns>
        public static RangeBucket CreateRangeBucketFromDataRow(DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var range = Convert.ToString(row[(int)BucketColumnNames.SummarizeByColumn]);
            var count = row[BucketColumnNames.Count];

            // Ignore the row for "other" records, that did not match the ranges
            if (range == BucketColumnNames.RangeDefaultBucket)
            {
                return null;
            }

            // Parse the range
            var splitRange = range
                            .Split('-')
                            .Select(s => string.IsNullOrEmpty(s) ? (double?)null : double.Parse(s))
                            .ToArray();
            var from = splitRange[0];
            var to = splitRange[1];

            // Assemble the key
            // An empty limit becomes "*"
            // An integer limit is suffixed with ".0"
            var fromKey = from?.ToString("0.0##########", CultureInfo.InvariantCulture) ?? "*";
            var toKey = to?.ToString("0.0##########", CultureInfo.InvariantCulture) ?? "*";

            string key = $"{fromKey}-{toKey}";

            // Assemble the bucket
            var rb = new RangeBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = key,
                From = from,
                To = to,
                Aggs = new Dictionary<string, Dictionary<string, object>>(),
            };

            CreateAggregationColumns(rb, row, logger);

            return rb;
        }

        /// <summary>
        /// Create a new <see cref="DateRangeBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new DateRangeBucket.</returns>
        public static DateRangeBucket CreateDateRangeBucketFromDataRow(DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var range = Convert.ToString(row[(int)BucketColumnNames.SummarizeByColumn]);
            var count = row[BucketColumnNames.Count];

            // Ignore the row for "other" records, that did not match the ranges
            if (range == BucketColumnNames.RangeDefaultBucket)
            {
                return null;
            }

            // Assemble the bucket
            var drb = new DateRangeBucket
            {
                DocCount = Convert.ToInt32(count),
                Aggs = new Dictionary<string, Dictionary<string, object>>(),
            };

            // Parse the range
            var splitRange = range
                            .Split('_')
                            .Select(s => string.IsNullOrEmpty(s) ? (DateTime?)null : DateTime.Parse(s).ToUniversalTime())
                            .ToArray();
            var from = splitRange[0];
            var to = splitRange[1];

            if (from != null)
            {
                drb.From = TimeUtils.ToEpochMilliseconds(from.Value);
                drb.FromAsString = from?.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
            }

            if (to != null)
            {
                drb.To = TimeUtils.ToEpochMilliseconds(to.Value);
                drb.ToAsString = to?.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
            }

            drb.Key = $"{drb.FromAsString}-{drb.ToAsString}";

            CreateAggregationColumns(drb, row, logger);

            return drb;
        }

        public static void CreateAggregationColumns(Bucket bucket, DataRow row, ILogger logger)
        {
            // TODO: refactor the columns handling based on the Percentiles code.
            // See: workitem 15724
            var columns = row.Table.Columns;
            foreach (DataColumn column in columns)
            {
                if (column.ColumnName == BucketColumnNames.Count || columns.IndexOf(column) == (int)BucketColumnNames.SummarizeByColumn)
                {
                    continue;
                }

                var columnNameInfo = column.ColumnName.Split('%');

                if (columnNameInfo.Length > 1)
                {
                    // key%metric%value1%value2%keyed
                    var key = columnNameInfo[0];
                    var metric = columnNameInfo[1];

                    // extract the percentiles values: from the second to the last-1
                    var queryValues = columnNameInfo[2..^1];

                    // extract the boolean: last item of the pattern elements array
                    var keyed = bool.Parse(columnNameInfo[^1]);

                    if (metric != "percentile")
                    {
                        continue;
                    }
                    else
                    {
                        bucket.Aggs[key] = new Dictionary<string, object>();
                        var returnValues = new Dictionary<string, double>();

                        var percentileValues = (JArray)row[column.ColumnName];

                        foreach (var (name, value) in queryValues.Zip(percentileValues))
                        {
                            logger.LogTrace("Adding Percentile {name}:{value}", name, value);
                            returnValues.Add(name, value.Value<double>());
                        }

                        if (keyed)
                        {
                            // keyed ====> Dictionary<string, double>
                            bucket.Aggs[key].Add("values", returnValues);
                        }
                        else
                        {
                            // not keyed ====> List<KeyValuePair<double,double>>
                            bucket.Aggs[key].Add("values", returnValues.ToDictionary(item => double.Parse(item.Key, CultureInfo.InvariantCulture), item => item.Value).ToList());
                        }
                    }
                }
                else
                {
                    var columnName = column.ColumnName;
                    logger.LogTrace("Defining the value for {columnName}", columnName);

                    var cell = row[columnName].ToString();
                    if (string.IsNullOrEmpty(cell) ||
                        !double.TryParse(cell, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                    {
                        value = 0;
                    }

                    bucket.Aggs[columnName] = new Dictionary<string, object>() {
                        { "value", value },
                    };
                }
            }
        }
    }
}