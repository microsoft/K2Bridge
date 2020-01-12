// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;
    using System.Data;
    using Newtonsoft.Json;

    public class DateHistogramBucket : IBucket
    {
        private enum ColumnNames
        {
            Timestamp = 0,
            Count = 1,
        }

        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("key_as_string")]
        public string KeyAsString { get; set; }

        /// <summary>
        /// Create a new <see cref="DateHistogramBucket" from a given <see cref="DataRow"/>/>.
        /// </summary>
        /// <param name="row">The row to be transformed to bucket.</param>
        /// <returns>A new DateHistogramBucket.</returns>
        public static DateHistogramBucket Create(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            var timestamp = row[(int)ColumnNames.Timestamp];
            var count = row[(int)ColumnNames.Count];
            var dateBucket = (DateTime)timestamp;

            return new DateHistogramBucket
            {
                DocCount = Convert.ToInt32(count),
                Key = Convert.ToInt64(dateBucket.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
            };
        }
    }
}
