// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations.Metric
{
    using Newtonsoft.Json;

    /// <summary>
    /// A base class which defines a metric aggregation can be used with partition operator.
    /// </summary>
    internal abstract class PartitionableMetricAggregation : MetricAggregation
    {
        [JsonIgnore]
        public string PartitionKey { get; set; }
    }
}
