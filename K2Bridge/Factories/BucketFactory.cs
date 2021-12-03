// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.RegularExpressions;
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
                Aggs = new Dictionary<string, Dictionary<string, dynamic>>(),
            };

            var clmns = row.Table.Columns;
            foreach (DataColumn clmn in clmns)
            {
                if (clmn.ColumnName == BucketColumnNames.Count || clmns.IndexOf(clmn) == (int)BucketColumnNames.Timestamp)
                {
                    continue;
                }

                // Step 1: create new Regex.
                Regex regex = new Regex(@"^_(\d+)%*(100\.00|[0-9]?[0-9]\.[0-9]{1})$");

                // Step 2: call Match on Regex instance.
                Match match = regex.Match(clmn.ColumnName);

                if (match.Success)
                {
                    var percentile = match.Groups[2].Value;

                    if (!dhb.Aggs.ContainsKey(match.Groups[1].Value))
                    {
                        dhb.Aggs[match.Groups[1].Value] = new Dictionary<string, dynamic>();
                    }

                    dhb.Aggs[match.Groups[1].Value].Add(percentile, Convert.ToDouble(row[clmn.ColumnName]));
                }
                else
                {
                    dhb.Aggs[clmn.ColumnName.Substring(1)] = new Dictionary<string, dynamic>() {
                        { "value", Convert.ToDouble(row[clmn.ColumnName]) },
                    };

                    // new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
                }
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

                tb.Aggs[clmn.ColumnName] = new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
            }

            return tb;
        }
    }
}
