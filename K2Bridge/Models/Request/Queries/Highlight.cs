// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the highlighting tags for matching terms.
    /// </summary>
    internal class Highlight
    {
        /// <summary>
        /// Gets or sets pre tags.
        /// </summary>
        [JsonProperty("pre_tags")]
        public List<string> PreTags { get; set; }

        /// <summary>
        /// Gets or sets post tags.
        /// </summary>
        [JsonProperty("post_tags")]
        public List<string> PostTags { get; set; }
    }
}
