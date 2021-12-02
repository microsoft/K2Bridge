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
    [JsonConverter(typeof(TermsAggregationConverter))]
    internal class TermsAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the field to aggregate.
        /// </summary>
        [JsonProperty("field")]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the field name used to sort buckets.
        /// </summary>
        public string SortFieldName { get; set; } = "count_";

        /// <summary>
        /// Gets or sets the ordering of the results.
        /// </summary>
        public string SortOrder { get; set; } = "desc";

        /// <summary>
        /// Gets or sets the number of buckets to return.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; set; } = 10;

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
