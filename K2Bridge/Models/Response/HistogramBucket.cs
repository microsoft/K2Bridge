// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// histogram bucket response.
    /// </summary>
    [JsonConverter(typeof(HistogramBucketAggsConverter))]
    public class HistogramBucket : Bucket
    {
        /// <summary>
        /// Gets or sets the timestamp bucket key.
        /// This key is a 64 bit number representing a timestamp in milliseconds-since-the-epoch .
        /// </summary>
        [JsonProperty("key")]
        public new long Key { get; set; }

        public bool Keyed { get; set; }
    }
}