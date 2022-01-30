// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.ElasticError;

/// <summary>
/// Elastic Error response class.
/// Note that this class should be merged with ElasticResponse. more info in this bug:
/// https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1759.
/// </summary>
public class ElasticErrorResponse
{
    private readonly List<ErrorResponseElement> responses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticErrorResponse"/> class.
    /// </summary>
    /// <param name="type">General error type.</param>
    /// <param name="reason">General reason for error.</param>
    /// <param name="phase">Phase when error happend.</param>
    public ElasticErrorResponse(string type, string reason, string phase)
    {
        responses.Add(new ErrorResponseElement(type, reason, phase));
    }

    /// <summary>
    /// Gets the Response elements.
    /// </summary>
    [JsonProperty("responses")]
    public IEnumerable<ErrorResponseElement> Responses => responses;

    /// <summary>
    /// Adds a root-cause element to the error response.
    /// </summary>
    /// <param name="type">Specific error type.</param>
    /// <param name="reason">Specific reason for error.</param>
    /// <param name="indexName">Index name where error happend.</param>
    public void AddRootCause(string type, string reason, string indexName) =>
        responses[0].Error.AddRootCause(new RootCause(type, reason, indexName, indexName));

    /// <summary>
    /// Fluent extension over AddRootCause.
    /// </summary>
    /// <param name="type">Specific error type.</param>
    /// <param name="reason">Specific reason for error.</param>
    /// <param name="indexName">Index name where error happend.</param>
    /// <returns>The instance of ElasticErrorResponse.</returns>
    public ElasticErrorResponse WithRootCause(string type, string reason, string indexName)
    {
        AddRootCause(type, reason, indexName);
        return this;
    }
}
