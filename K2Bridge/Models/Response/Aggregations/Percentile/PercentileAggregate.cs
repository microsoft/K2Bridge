// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes percentile aggregate response element.
    /// </summary>
    [JsonConverter(typeof(PercentileAggregateConverter))]
    public class PercentileAggregate : IAggregate
    {
        /// <summary>
        /// Gets the list of PercentileItem values.
        /// </summary>
        public List<PercentileItem> Values { get; } = new List<PercentileItem>();

        /// <summary>
        /// Gets or sets a value indicating whether this is a Keyed value.
        /// </summary>
        public bool Keyed { get; set; }
    }
}