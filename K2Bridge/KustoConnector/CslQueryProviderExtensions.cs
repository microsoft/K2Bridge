// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using Kusto.Data.Common;

    public static class CslQueryProviderExtensions
    {
        public static (TimeSpan timeTaken, IDataReader reader) ExecuteMonitoredQuery(this ICslQueryProvider client, string query)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var reader = client.ExecuteQuery(query);
            stopwatch.Stop();
            return (stopwatch.Elapsed, reader);
        }
    }
}
