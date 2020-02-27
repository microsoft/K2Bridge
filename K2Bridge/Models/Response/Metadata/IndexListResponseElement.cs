// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    /// <summary>
    /// Indices collecton response element.
    /// </summary>
    public class IndexListResponseElement : ResponseElementBase
    {
        /// <summary>
        /// Gets or sets index aggregations.
        /// </summary>
        [JsonProperty("aggregations")]
        public IndexListAggregations Aggregations { get; set; } = new IndexListAggregations();
    }
}