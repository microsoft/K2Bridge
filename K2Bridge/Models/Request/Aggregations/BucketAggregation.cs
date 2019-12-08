// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Aggregations
{
    internal abstract class BucketAggregation : LeafAggregation
    {
        public string Metric { get; set; } = "count()";
    }
}
