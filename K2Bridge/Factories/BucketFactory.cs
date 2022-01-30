// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using K2Bridge.Models;
    using K2Bridge.Models.Response.Aggregations;
    using K2Bridge.Models.Response.Aggregations.Bucket;
    using K2Bridge.Utils;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Bucket Factory.
    /// </summary>
    internal static class BucketFactory
    {
        /// <summary>
        /// Create a new <see cref="DateHistogramBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns><see cref="DateHistogramBucket"/> instance.</returns>
        public static DateHistogramBucket CreateDateHistogramBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var timestamp = row[primaryKey];

            var dhb = new DateHistogramBucket();

            if (timestamp is DBNull)
            {
                return dhb;
            }

            var count = row[BucketColumnNames.Count];
            var dateBucket = (DateTime)timestamp;

            dhb = new DateHistogramBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = TimeUtils.ToEpochMilliseconds(dateBucket),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
            };

            dhb.AddAggregates(primaryKey, row, query, logger);

            return dhb;
        }

        /// <summary>
        /// Create a new <see cref="TermsBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns><see cref="TermsBucket"/> instance.</returns>
        public static TermsBucket CreateTermsBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var key = row[primaryKey];
            var count = row[BucketColumnNames.Count];

            var tb = new TermsBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToString(key),
            };

            tb.AddAggregates(primaryKey, row, query, logger);

            return tb;
        }

        /// <summary>
        /// Create a new <see cref="RangeBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns><see cref="RangeBucket"/> instance.</returns>
        public static RangeBucket CreateRangeBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var range = Convert.ToString(row[primaryKey]);
            var count = row[BucketColumnNames.Count];

            // Ignore the row for "other" records, that did not match the ranges
            if (range == BucketColumnNames.RangeDefaultBucket)
            {
                return null;
            }

            // Parse the range
            var splitRange = range
                            .Split(AggregationsConstants.MetadataSeparator)
                            .Select(s => string.IsNullOrEmpty(s) ? (double?)null : double.Parse(s))
                            .ToArray();
            var from = splitRange[0];
            var to = splitRange[1];

            // Assemble the key
            // An empty limit becomes "*"
            // An integer limit is suffixed with ".0"
            var fromKey = from?.ToString("0.0##########", CultureInfo.InvariantCulture) ?? "*";
            var toKey = to?.ToString("0.0##########", CultureInfo.InvariantCulture) ?? "*";

            var key = $"{fromKey}-{toKey}";

            // Assemble the bucket
            var rb = new RangeBucket
            {
                DocCount = count == DBNull.Value ? 0 : Convert.ToInt32(count),
                Key = key,
                From = from,
                To = to,
            };

            rb.AddAggregates(primaryKey, row, query, logger);

            return rb;
        }

        /// <summary>
        /// Create a new <see cref="FiltersBucket"/> from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns>A new FiltersBucket.</returns>
        public static FiltersBucket CreateFiltersBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            // We can't use primaryKey here since the Kusto column name contains the encoded filters
            // Instead, we pick the column name by index (first column)
            var encodedKey = row.Table.Columns[0].ColumnName;

            // Access the column data  via its name
            var filter = Convert.ToString(row[encodedKey]);
            var count = row[BucketColumnNames.Count];

            // Assemble the bucket
            var fb = new FiltersBucket
            {
                Key = filter,
                DocCount = count == DBNull.Value ? 0 : Convert.ToInt32(count),
            };

            // Pass the encoded column name so that it is correctly identified when adding other aggs
            fb.AddAggregates(encodedKey, row, query, logger);

            return fb;
        }

        /// <summary>
        /// Create a new <see cref="DateRangeBucket"/> from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns>A new DateRangeBucket.</returns>
        public static DateRangeBucket CreateDateRangeBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var range = Convert.ToString(row[primaryKey]);
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
            };

            // Parse the range
            var splitRange = range
                            .Split(AggregationsConstants.MetadataSeparator)
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

            drb.AddAggregates(primaryKey, row, query, logger);

            return drb;
        }

        /// <summary>
        /// Create a new <see cref="HistogramBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="query">QueryData containing query information.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        /// <returns>A new HistogramBucket.</returns>
        public static HistogramBucket CreateHistogramBucket(string primaryKey, DataRow row, QueryData query, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var key = row[0];

            var hb = new HistogramBucket();

            if (key is DBNull)
            {
                return hb;
            }

            var count = row[BucketColumnNames.Count];

            hb = new HistogramBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToDouble(key),
            };

            hb.AddAggregates(primaryKey, row, query, logger);

            return hb;
        }
    }
}
