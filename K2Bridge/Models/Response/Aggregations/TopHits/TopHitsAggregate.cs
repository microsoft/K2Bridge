// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes tophits aggregate response element.
    /// </summary>
    public class TopHitsAggregate : IAggregate
    {
        /// <summary>
        /// Gets or sets the totalhits object.
        /// </summary>
        [JsonProperty("total")]
        public TotalHits Total { get; set; }

        /// <summary>
        /// Gets or sets the MaxScore value.
        /// </summary>
        [JsonProperty("max_score")]
        public double? MaxScore { get; set; }

        /// <summary>
        /// Gets the hits collection.
        /// </summary>
        [JsonProperty("hits")]
        public HitsCollection Hits { get; } = new HitsCollection();
    }
}