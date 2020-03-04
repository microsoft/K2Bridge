// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// Dynamically built bucket based on a unique value of the searched term.
    /// </summary>
    public class TermBucket : IBucket
    {
        /// <summary>
        /// Gets or sets document count.
        /// </summary>
        [JsonProperty("doc_count")]
        public int DocCount { get; set; }

        /// <summary>
        /// Gets or sets the unique key.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
