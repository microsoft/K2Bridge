// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL;

using K2Bridge.Models;
using Kusto.Data.Common;

/// <summary>
/// Extension methods for ClientRequestProperties.
/// </summary>
public static class ClientRequestPropertiesExtensions
{
    /// <summary>
    /// Constructs Kusto's ClientRequestProperties from this RequestContext.
    /// </summary>
    /// <param name="appName">Name of this application to be used in ClientRequestId.</param>
    /// <param name="activityName">Activity identifier to be used in ClientRequestId.</param>
    /// <param name="requestContext">The object containing the properties with which ClientRequestProperties will be set.</param>
    /// <returns>Kusto's <see cref="ClientRequestProperties"/> object.</returns>
    public static ClientRequestProperties ConstructClientRequestPropertiesFromRequestContext(string appName, string activityName, RequestContext requestContext)
    {
        Ensure.IsNotNull(requestContext, nameof(requestContext));

        // TODO: When a single K2 flow will generate multiple requests to Kusto - find a way to differentiate them using different ClientRequestIds
        return new ClientRequestProperties
        {
            ClientRequestId = $"{ConstructKustoPrefix(appName, activityName)}{requestContext.CorrelationId}",
        };
    }

    private static string ConstructKustoPrefix(string appName, string activityName)
    {
        return $"{appName}.{activityName};";
    }
}
