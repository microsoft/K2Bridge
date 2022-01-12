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
        /// Gets or sets response hits.
        /// </summary>
        [JsonProperty("hits")]
        public HitsCollection Hits { get; set; } = new HitsCollection();
    }
}