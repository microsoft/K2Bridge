// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// histogram bucket response.
    /// </summary>
    [JsonConverter(typeof(HistogramBucketConverter))]
    public class HistogramBucket : KeyedBucket
    {
        /// <summary>
        /// Gets or sets the Keyed value.
        /// </summary>
        public bool Keyed { get; set; } = false;
    }
}
