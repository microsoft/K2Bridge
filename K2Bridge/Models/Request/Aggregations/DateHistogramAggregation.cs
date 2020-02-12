// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(DateHistogramAggregationConverter))]
    internal class DateHistogramAggregation : BucketAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
