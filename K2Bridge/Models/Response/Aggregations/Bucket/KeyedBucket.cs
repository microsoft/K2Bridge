// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes keyed bucket response element.
    /// </summary>
    public class KeyedBucket<TKey> : BucketBase
    {
        [JsonProperty("key")]
        public TKey Key { get; set; }

        [JsonProperty("key_as_string")]
        public string KeyAsString { get; set; }
    }
}