// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using Newtonsoft.Json;

    internal abstract class MetricAggregation : Aggregation
    {
        /// <summary>
        /// Gets or sets the field on which to aggregate.
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the value to use when the aggregation finds a missing value in a document.
        /// </summary>
        [JsonProperty("missing")]
        public double? Missing { get; set; }
    }
}
