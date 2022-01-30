// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Models.Response.Aggregations;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response;

/// <summary>
/// Response element.
/// </summary>
public class ResponseElement : ResponseElementBase
{
    /// <summary>
    /// Gets or sets aggregations in a response element.
    /// </summary>
    [JsonProperty("aggregations")]
    public AggregateDictionary Aggregations { get; } = new AggregateDictionary();
}
