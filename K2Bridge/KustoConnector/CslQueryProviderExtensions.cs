// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Kusto.Data.Common;
    using Prometheus;

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
        /// <param name="queryMetric">Prometheus query duration metric.</param>
        /// <returns>Tuple of timeTaken and the reader result.</returns>
        public static async Task<(TimeSpan timeTaken, IDataReader reader)> ExecuteMonitoredQueryAsync(
            this ICslQueryProvider client,
            string query,
            IHistogram queryMetric)
        {
            Ensure.IsNotNull(client, nameof(client));
            Ensure.IsNotNull(queryMetric, nameof(client));
            Ensure.IsNotNullOrEmpty(query, nameof(query));

            // Timer to be used to report the duration of a query to.
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var reader = await client.ExecuteQueryAsync(string.Empty, query, null);
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;

            queryMetric.Observe(duration.TotalSeconds);

            return (duration, reader);
        }
    }
}
