// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    internal abstract class MetricAggregation : LeafAggregation
    {
        [JsonProperty("field")]
        public string FieldName { get; set; }
    }
}
