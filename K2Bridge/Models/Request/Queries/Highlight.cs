// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace K2Bridge.Models.Request.Queries;

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
