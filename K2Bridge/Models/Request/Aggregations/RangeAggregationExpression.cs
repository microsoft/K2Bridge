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
    internal class RangeAggregationExpression
    {
        /// <summary>
        /// Gets or sets the from value.
        /// </summary>
        [JsonProperty("from")]
        public double? From { get; set; }

        /// <summary>
        /// Gets or sets the to value.
        /// </summary>
        [JsonProperty("to")]
        public double? To { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
