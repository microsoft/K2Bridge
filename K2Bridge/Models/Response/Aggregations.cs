// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class Aggregations
    {
        [JsonProperty("2")]
        public BucketsCollection Collection { get; set; } = new BucketsCollection();
    }
}
