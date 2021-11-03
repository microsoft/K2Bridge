// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Metadata
{
    using Newtonsoft.Json;

    /// <summary>
    /// Index element of the resolve index response.
    /// </summary>
    public class ResolveIndexResponseIndex
    {
        /// <summary>
        /// Gets or sets the index name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
