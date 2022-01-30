// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response;

using Newtonsoft.Json;

/// <summary>
/// Object containing a count of shards used for the request.
/// </summary>
public class Shards
{
    /// <summary>
    /// Gets or sets the total number of shards.
    /// Exists in order to support Kibana's expected response (default = 1).
    /// </summary>
    [JsonProperty("total")]
    public int Total { get; set; } = 1;

    /// <summary>
    /// Gets or sets the indication of success.
    /// Exists in order to support Kibana's expected response (default = 1).
    /// </summary>
    [JsonProperty("successful")]
    public int Successful { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of shards that skipped the request.
    /// It is here only to support the expected response.
    /// </summary>
    [JsonProperty("skipped")]
    public int Skipped { get; set; }

    /// <summary>
    /// Gets or sets the number of shards that failed to execute the request.
    /// It is here only to support the expected response.
    /// </summary>
    [JsonProperty("failed")]
    public int Failed { get; set; }
}
