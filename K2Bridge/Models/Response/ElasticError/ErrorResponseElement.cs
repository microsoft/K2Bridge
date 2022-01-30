// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.ElasticError;

using Newtonsoft.Json;

/// <summary>
/// Error Response Element classs.
/// </summary>
public class ErrorResponseElement
{
    private const int ErrorStatus = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseElement"/> class.
    /// </summary>
    /// <param name="type">General error type.</param>
    /// <param name="reason">General reason for error.</param>
    /// <param name="phase">Phase when error happend.</param>
    public ErrorResponseElement(string type, string reason, string phase)
    {
        Error = new ErrorElement(type, reason, phase);
    }

    /// <summary>
    /// Gets the Error Element.
    /// </summary>
    [JsonProperty("error")]
    public ErrorElement Error { get; }

    /// <summary>
    /// Gets the Status Element.
    /// </summary>
    [JsonProperty("status")]
    public int Status { get; } = ErrorStatus;
}
