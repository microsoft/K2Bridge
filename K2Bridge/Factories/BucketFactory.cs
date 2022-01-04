// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Linq;
    using System.Data;
    using System.Globalization;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;
    using Microsoft.Extensions.Logging;
    using K2Bridge.Models.Response.Aggregations;

    /// <summary>
    /// Bucket Factory.
    /// </summary>
    internal static class BucketFactory
    {
        /// <summary>
        /// Declare a delegate used to create a bucket instance of type TBucket from a given <see cref="DataRow"/>.
        /// </summary>
        /// <typeparam name="TBucket">The bucket type.</typeparam>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">The <see cref="ILogger"> used for logging.</param>
        /// <returns>TBucket instance.</returns>
        public delegate TBucket CreateBucket<TBucket>(string primaryKey, DataRow row, ILogger logger);

        /// <summary>
        /// Create a new <see cref="DateHistogramBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">The <see cref="ILogger"> used for logging.</param>
        /// <returns><see cref="DateHistogramBucket"> instance.</returns>
        public static DateHistogramBucket CreateDateHistogramBucket(string primaryKey, DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var timestamp = row[primaryKey];
            var count = row[BucketColumnNames.Count];
            var dateBucket = (DateTime)timestamp;

            var dhb = new DateHistogramBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = TimeUtils.ToEpochMilliseconds(dateBucket),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
            };

            dhb.AddAggregates(primaryKey, row, logger);

            return dhb;
        }

        /// <summary>
        /// Create a new <see cref="TermsBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">The <see cref="ILogger"> used for logging.</param>
        /// <returns><see cref="TermsBucket"> instance.</returns>
        public static TermsBucket CreateTermsBucket(string primaryKey, DataRow row, ILogger logger)
        {
            Ensure.IsNotNull(row, nameof(row));

            var key = row[primaryKey];
            var count = row[BucketColumnNames.Count];

            var tb = new TermsBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToString(key),
            };

            tb.AddAggregates(primaryKey, row, logger);

            return tb;
        }

        /// <summary>
        /// Create a new <see cref="RangeBucket" from a given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="primaryKey">The primary aggregation key.</param>
        /// <param name="row">The row to be parsed.</param>
        /// <param name="logger">The <see cref="ILogger"> used for logging.</param>
        /// <returns><see cref="RangeBucket"> instance.</returns>
        public static RangeBucket CreateRangeBucket(string primaryKey, DataRow row, ILogger logger)
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
            };

            rb.AddAggregates(primaryKey, row, logger);

            return rb;
        }
    }
}