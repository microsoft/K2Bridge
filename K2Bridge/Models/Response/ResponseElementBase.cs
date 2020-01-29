// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public abstract class ResponseElementBase
    {
        private const int STATUS = 200;

        [JsonProperty("took")]
        public int TookMilliseconds { get; set; }

        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        [JsonProperty("_shards")]
        public Shards Shards { get; set; } = new Shards();

        [JsonProperty("hits")]
        public HitsCollection Hits { get; set; } = new HitsCollection();

        [JsonProperty("status")]
        public int Status { get; set; } = STATUS;

        [JsonProperty("_backendQuery", NullValueHandling = NullValueHandling.Ignore)]

        public string BackendQuery { get; set; }
    }
}
