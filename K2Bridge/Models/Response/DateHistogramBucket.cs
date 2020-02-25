// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    public class DateHistogramBucket : IBucket
    {
        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("key_as_string")]
        public string KeyAsString { get; set; }
    }
}
