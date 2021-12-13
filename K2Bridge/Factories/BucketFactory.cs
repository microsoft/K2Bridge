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
                Aggs = new Dictionary<string, Dictionary<string, object>>(),
            };

            var clmns = row.Table.Columns;
            foreach (DataColumn clmn in clmns)
            {
                if (clmn.ColumnName == BucketColumnNames.Count || clmns.IndexOf(clmn) == (int)BucketColumnNames.Timestamp)
                {
                    continue;
                }

                var columnNameInfo = clmn.ColumnName.Split('%');

                if (columnNameInfo.Length > 1)
                {
                    // key%metric%value1%value2%keyed
                    var key = columnNameInfo[0];
                    var metric = columnNameInfo[1];
                    var queryValues = columnNameInfo[2..^1];
                    var keyed = bool.Parse(columnNameInfo[^1]);

                    if (metric == "percentile")
                    {
                        dhb.Aggs[key] = new Dictionary<string, object>();
                        var returnValues = new Dictionary<string, double>();

                        var percentileValues = (JArray)row[clmn.ColumnName];

                        for (var index = 0; index < queryValues.Length; index++)
                        {
                            var name = queryValues[index];
                            var value = percentileValues[index].ToString();
                            returnValues.Add(name, double.Parse(value, CultureInfo.InvariantCulture));
                        }

                        if (keyed)
                        {
                            // keyed ====> Dictionary<string, double>
                            dhb.Aggs[key].Add("values", returnValues);
                        }
                        else
                        {
                            // not keyed ====> List<object(double,double)>
                            dhb.Aggs[key].Add("values", returnValues.ToDictionary(item => double.Parse(item.Key, CultureInfo.InvariantCulture), item => item.Value).ToList());
                        }
                    }
                }
                else
                {
                    var columnName = clmn.ColumnName;
                    logger.LogTrace("Defining the value for {columnName}", columnName);

                    dhb.Aggs[columnName] = new Dictionary<string, object>() {
                        { "value", Convert.ToDouble(row[columnName]) },
                    };
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
