// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    public class IndexListResponseElement : ResponseElementBase
    {
        [JsonProperty("aggregations")]
        public IndexListAggregations Aggregations { get; set; } = new IndexListAggregations();
    }
}