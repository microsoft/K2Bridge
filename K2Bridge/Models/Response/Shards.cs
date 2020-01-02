// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class Shards
    {
        [JsonProperty("total")]
        public int Total { get; set; } = 1;

        [JsonProperty("successful")]
        public int Successful { get; set; } = 1;

        [JsonProperty("skipped")]
        public int Skipped { get; set; }

        [JsonProperty("failed")]
        public int Failed { get; set; }
    }
}
