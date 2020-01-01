// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(AggregationFieldConverter))]
    internal class AvgAggregation : MetricAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
