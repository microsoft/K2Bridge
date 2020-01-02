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
        private const string TimestampColumn = "timestamp";
        private const string CountColumn = "count_";

        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("key_as_string")]
        public string KeyAsString { get; set; }

        public static DateHistogramBucket Create(DataRow row)
        {
            var f0 = row[TimestampColumn];
            var f1 = row[CountColumn];
            var dateBucket = (DateTime)f0;

            return new DateHistogramBucket
            {
                DocCount = Convert.ToInt32(f1),
                Key = Convert.ToInt64(dateBucket.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds),
                KeyAsString = dateBucket.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz").Remove(26, 1), // this should be converted to the timezone requested (all others in utc)
            };
        }
    }
}
