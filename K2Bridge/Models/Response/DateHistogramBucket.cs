// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Date histogram bucket response.
    /// </summary>
    [JsonConverter(typeof(DateHistogramBucketAggsConverter))]
    public class DateHistogramBucket : IBucket
    {
        /// <summary>
        /// Gets or sets document count.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp bucket key.
        /// This key is a 64 bit number representing a timestamp in milliseconds-since-the-epoch .
        /// </summary>
        [JsonProperty("key")]
        public long Key { get; set; }

        /// <summary>
        /// Gets or sets the timestamp bucket key.
        /// The key here is represented as a date string.
        /// </summary>
        [JsonProperty("key_as_string")]
        public string KeyAsString { get; set; }

        /// <summary>
        /// Gets or Sets aggegations dictionary.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Dictionary<string, double>> Aggs { get; set; }
    }
}
