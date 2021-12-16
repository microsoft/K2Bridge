// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Terms bucket response.
    /// </summary>
    [JsonConverter(typeof(DateRangeBucketAggsConverter))]
    public class DateRangeBucket : Bucket
    {
        /// <summary>
        /// Gets or sets the from value.
        /// </summary>
        [JsonProperty("from")]
        public long? From { get; set; }

        /// <summary>
        /// Gets or sets the from value as string.
        /// </summary>
        [JsonProperty("from_as_string")]
        public string FromAsString { get; set; }

        /// <summary>
        /// Gets or sets the to value.
        /// </summary>
        [JsonProperty("to")]
        public long? To { get; set; }

        /// <summary>
        /// Gets or sets the to value as string.
        /// </summary>
        [JsonProperty("to_as_string")]
        public string ToAsString { get; set; }
    }
}
