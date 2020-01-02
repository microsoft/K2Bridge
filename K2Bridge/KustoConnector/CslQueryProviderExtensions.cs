// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using Kusto.Data.Common;

    /// <summary>
    /// Extension methods for Kusto query operation.
    /// </summary>
    public static class CslQueryProviderExtensions
    {
        public static (TimeSpan timeTaken, IDataReader reader) ExecuteMonitoredQuery(
            this ICslQueryProvider client,
            string query)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var reader = client.ExecuteQuery(query);
            sw.Stop();
            return (sw.Elapsed, reader);
        }
    }
}
