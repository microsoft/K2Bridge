// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.ElasticError;

using Newtonsoft.Json;

/// <summary>
/// Root Cause class.
/// </summary>
public class RootCause
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootCause"/> class.
    /// </summary>
    /// <param name="type">Type of error.</param>
    /// <param name="reason">Reason for error.</param>
    /// <param name="index">Index where error happend.</param>
    /// <param name="indexUuid">Index UUID where error happend.</param>
    public RootCause(string type, string reason, string index, string indexUuid)
    {
        Type = type;
        Reason = reason;
        Index = index;
        IndexUuid = indexUuid;
    }

    /// <summary>
    /// Gets the type of error.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; }

    /// <summary>
    /// Gets the reason for error.
    /// </summary>
    [JsonProperty("reason")]
    public string Reason { get; }

    /// <summary>
    /// Gets the index where error happend.
    /// </summary>
    [JsonProperty("index_uuid")]
    public string IndexUuid { get; }

    /// <summary>
    /// Gets the index UUID where error happend.
    /// </summary>
    [JsonProperty("index")]
    public string Index { get; }
}
