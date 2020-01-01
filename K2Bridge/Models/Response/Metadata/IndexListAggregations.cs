// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    public class IndexListAggregations
    {
        [JsonProperty("indices")]
        public BucketsCollection IndexCollection { get; set; } = new BucketsCollection();
    }
}
