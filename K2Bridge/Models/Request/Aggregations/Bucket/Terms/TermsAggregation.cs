// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Bucket aggregation that creates one bucket per unique value in the designated field.
    // Default values are taken from Elasticserch API documentation.
    /// </summary>
    public class TermsAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the field to aggregate.
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; internal set; }

        [JsonProperty("order")]
        [JsonConverter(typeof(TermsOrderConverter))]
        public TermsOrder Order { get; internal set; }

        /// <summary>
        /// Gets or sets the number of buckets to return.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; internal set; } = 10;

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
