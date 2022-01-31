// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoDAL;

using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using K2Bridge.Telemetry;
using Kusto.Data.Common;

/// <summary>
/// Extension methods for Kusto query operation.
/// </summary>
public static class CslQueryProviderExtensions
{
    /// <summary>
    /// Execute monitored ADX query.
    /// </summary>
    /// <param name="client">Query provider.</param>
    /// <param name="query">ADX query string.</param>
    /// <param name="clientRequestProperties">An object that represents properties that will be sent to Kusto.</param>
    /// <param name="metrics">Prometheus query duration metric.</param>
    /// <returns>Tuple of timeTaken and the reader result.</returns>
    public static async Task<(TimeSpan TimeTaken, IDataReader Reader)> ExecuteMonitoredQueryAsync(
        this ICslQueryProvider client,
        string query,
        ClientRequestProperties clientRequestProperties,
        Metrics metrics)
    {
        Ensure.IsNotNull(client, nameof(client));
        Ensure.IsNotNull(metrics, nameof(metrics));
        Ensure.IsNotNullOrEmpty(query, nameof(query));

        // Timer to be used to report the duration of a query to.
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var reader = await client.ExecuteQueryAsync(string.Empty, query, clientRequestProperties);
        stopwatch.Stop();
        var duration = stopwatch.Elapsed;

        metrics?.AdxQueryDurationMetric.Observe(duration.TotalSeconds);

        return (duration, reader);
    }
}
