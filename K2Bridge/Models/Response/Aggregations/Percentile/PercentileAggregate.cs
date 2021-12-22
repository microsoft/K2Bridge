// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using K2Bridge.JsonConverters;

    /// <summary>
    /// Describes percentile aggregate response element.
    /// </summary>
    [JsonConverter(typeof(PercentileAggregateConverter))]
    public class PercentileAggregate : IAggregate
    {
        public List<PercentileItem> Values { get; } = new List<PercentileItem>();

        public bool Keyed { get; set; }
    }
}