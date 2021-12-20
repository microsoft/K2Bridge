// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes range bucket response element.
    /// </summary>
    public class RangeBucket : BucketBase
    {
        /// <summary>
        /// Gets or sets the from value.
        /// </summary>
        [JsonProperty("from")]
        public double? From { get; set; }

        /// <summary>
        /// Gets or sets the from_as_string value.
        /// </summary>
        [JsonProperty("from_as_string")]
        public string FromAsString { get; set; }

        /// <summary>
        /// Gets or sets the key value.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the to value.
        /// </summary>
        [JsonProperty("to")]
        public double? To { get; set; }

        /// <summary>
        /// Gets or sets the to_as_string value.
        /// </summary>
        [JsonProperty("to_as_string")]
        public string ToAsString { get; set; }
    }
}
