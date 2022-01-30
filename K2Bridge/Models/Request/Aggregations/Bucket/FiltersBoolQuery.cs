// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations.Bucket;

using K2Bridge.Models.Request.Queries;
using Newtonsoft.Json;

/// <summary>
/// A Bool query within a Filters aggregation.
/// </summary>
internal class FiltersBoolQuery
{
    /// <summary>
    /// Gets or sets the Bool query.
    /// </summary>
    [JsonProperty("bool")]
    public BoolQuery BoolQuery { get; set; }
}
