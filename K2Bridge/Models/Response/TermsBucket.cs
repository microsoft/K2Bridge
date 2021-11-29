// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using Newtonsoft.Json;

    /// <summary>
    /// Date histogram bucket response.
    /// </summary>
    [JsonConverter(typeof(TermsBucketAggsConverter))]
    public class TermsBucket : IBucket
    {
        /// <summary>
        /// Gets or sets document count.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        /// <summary>
        /// Gets or sets the bucket key.
        /// This key is a string containing the term.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets aggegations dictionary.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, List<double>> Aggs { get; set; }
    }
}
