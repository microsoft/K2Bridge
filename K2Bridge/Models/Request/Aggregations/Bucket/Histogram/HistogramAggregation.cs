// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// This multi-bucket aggregation is similar to the normal histogram, but it can only be used with date or date range values.
    /// </summary>
    internal class HistogramAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the field to target.
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the interval to use when bucketing documents.
        /// </summary>
        [JsonProperty("interval")]
        public int? Interval { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of documents that a bucket must contain to be returned in the response.
        /// The default is 0 meaning that buckets with no documents will be returned.
        /// </summary>
        [JsonProperty("min_doc_count")]
        public int MinimumDocumentCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets field keyed for the Histogram Aggregation.
        /// </summary>
        [JsonProperty("keyed")]
        public bool? Keyed { get; set; } = false;

        /// <summary>
        /// Gets or sets field extended_bounds for the Histogram Aggregation.
        /// </summary>
        [JsonProperty("extended_bounds")]
        public Bounds ExtendedBounds { get; set; }

        /// <summary>
        /// Gets or sets field hard_bounds for the Histogram Aggregation.
        /// </summary>
        [JsonProperty("hard_bounds")]
        public Bounds HardBounds { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}