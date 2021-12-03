// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Data;
    using System.Globalization;
    using K2Bridge.Models.Response;
    using K2Bridge.Utils;

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
        public static DateHistogramBucket CreateDateHistogramBucketFromDataRow(DataRow row)
        {
            Ensure.IsNotNull(row, nameof(row));

            // TODO: timestamp is always the first row (probably named _2), and count will be named _count
            // we currently mix index and column names, need to check if we can enhance this logic
            // workitem 15050
            var timestamp = row[(int)BucketColumnNames.Timestamp];
            var count = row[BucketColumnNames.Count];
            var dateBucket = (DateTime)timestamp;

            var dhb = new DateHistogramBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = TimeUtils.ToEpochMilliseconds(dateBucket),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                Aggs = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<double>>(),
            };

            var clmns = row.Table.Columns;
            foreach (DataColumn clmn in clmns)
            {
                if (clmn.ColumnName == BucketColumnNames.Count || clmns.IndexOf(clmn) == (int)BucketColumnNames.Timestamp)
                {
                    continue;
                }

                dhb.Aggs[clmn.ColumnName.Substring(1)] = new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
            }

            return dhb;
        }

        /// <summary>
        /// Create a new <see cref="TermsBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new TermsBucket.</returns>
        public static TermsBucket CreateTermsBucketFromDataRow(DataRow row)
        {
            Ensure.IsNotNull(row, nameof(row));

            var key = row[(int)BucketColumnNames.Timestamp];
            var count = row[BucketColumnNames.Count];

            var tb = new TermsBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToString(key),
                Aggs = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<double>>(),
            };

            var clmns = row.Table.Columns;
            foreach (DataColumn clmn in clmns)
            {
                if (clmn.ColumnName == BucketColumnNames.Count || clmns.IndexOf(clmn) == (int)BucketColumnNames.Timestamp)
                {
                    continue;
                }

                tb.Aggs[clmn.ColumnName.Substring(1)] = new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
            }

            return tb;
        }

        /// <summary>
        /// Create a new <see cref="RangeBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new TermsBucket.</returns>
        public static RangeBucket CreateRangeBucketFromDataRow(DataRow row)
        {
            Ensure.IsNotNull(row, nameof(row));

            var range = Convert.ToString(row[(int)BucketColumnNames.Timestamp]);
            var count = row[BucketColumnNames.Count];

            // Ignore the row for "other" records, that did not match the ranges
            if (range == BucketColumnNames.RangeDefaultBucket)
            {
                return null;
            }

            // Parse the range
            string[] splitRange = range.Split('-');
            double? from = string.IsNullOrEmpty(splitRange[0]) ? null : Convert.ToDouble(splitRange[0]);
            double? to = string.IsNullOrEmpty(splitRange[1]) ? null : Convert.ToDouble(splitRange[1]);

            // Assemble the key
            string key = from?.ToString("F1", CultureInfo.InvariantCulture) + "-" + to?.ToString("F1", CultureInfo.InvariantCulture);

            var rb = new RangeBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = key,
                From = from,
                To = to,
                Aggs = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<double>>(),
            };

            var clmns = row.Table.Columns;
            foreach (DataColumn clmn in clmns)
            {
                if (clmn.ColumnName == BucketColumnNames.Count || clmns.IndexOf(clmn) == (int)BucketColumnNames.Timestamp)
                {
                    continue;
                }

                rb.Aggs[clmn.ColumnName.Substring(1)] = new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
            }

            return rb;
        }
    }
}
