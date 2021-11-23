// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Factories
{
    using System;
    using System.Data;
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
        public static DateHistogramBucket CreateFromDataRow(DataRow row)
        {
            Ensure.IsNotNull(row, nameof(row));

            // fix column names
            var timestamp = row[(int)DateHistogramBucketColumnNames.Timestamp];
            var count = row["count_"];
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
                // todo: fix
                if (clmn.ColumnName == "count_" || clmns.IndexOf(clmn) == (int)DateHistogramBucketColumnNames.Timestamp)
                {
                    continue;
                }

                dhb.Aggs[clmn.ColumnName.Substring(1)] = new System.Collections.Generic.List<double>() { Convert.ToDouble(row[clmn.ColumnName]) };
            }

            return dhb;
        }
    }
}