// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Extended_bounds can "force" the histogram aggregation to start building buckets on a specific min value
    /// and also keep on building buckets up to a max value (even if there are no documents anymore).
    /// Using extended_bounds only makes sense when min_doc_count is 0 (the empty buckets will never be returned if min_doc_count is greater than 0).
    /// Hard_bounds can limit the range of buckets in the histogram.
    /// </summary>
    internal class Bounds
    {
        /// <summary>
        /// Gets or sets the field to target.
        /// </summary>
        [JsonProperty("min")]
        public long Min { get; set; }

        /// <summary>
        /// Gets or sets the field to target.
        /// </summary>
        [JsonProperty("max")]
        public long Max { get; set; }
    }
}
