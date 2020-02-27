 // Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Multi-bucket aggregation, similar to the normal histogram, but used only with date or date range values.
    /// </summary>
    [JsonConverter(typeof(DateHistogramAggregationConverter))]
    internal class DateHistogramAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the date field to aggregate.
        /// </summary>
        [JsonProperty("field")]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the date aggregation interval.
        /// </summary>
        [JsonProperty("interval")]
        public string Interval { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
