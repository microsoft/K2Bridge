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
#pragma warning disable SA1600 // Elements should be documented
        public static (TimeSpan timeTaken, IDataReader reader) ExecuteMonitoredQuery(
            this ICslQueryProvider client,
            string query)
#pragma warning restore SA1600 // Elements should be documented
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
#pragma warning disable CA1062 // Validate arguments of public methods
            var reader = client.ExecuteQuery(query);
#pragma warning restore CA1062 // Validate arguments of public methods
            sw.Stop();
            return (sw.Elapsed, reader);
        }
    }
}
