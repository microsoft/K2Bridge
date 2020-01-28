// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
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
        public static (TimeSpan timeTaken, IDataReader reader) ExecuteMonitoredQuery(
            this ICslQueryProvider client,
            string query,
            IHistogram queryMetric)
        {
            Ensure.IsNotNull(client, nameof(client));
            Ensure.IsNotNullOrEmpty(query, nameof(query));

            // Timer to be used to report the duration of a query to.
            using var timer = queryMetric.NewTimer();

            var reader = client.ExecuteQuery(query);

            return (timer.ObserveDuration(), reader);
        }
    }
}
