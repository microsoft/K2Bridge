// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoConnector
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using global::K2Bridge.KustoConnector;
    using global::K2Bridge.Telemetry;
    using Kusto.Data.Common;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class CslQueryProviderExtensionsTests
    {
        private readonly IDataReader data = Substitute.For<IDataReader>();
        private readonly ICslQueryProvider client = Substitute.For<ICslQueryProvider>();
        private readonly Metrics metric = Substitute.For<Metrics>();
        private readonly ClientRequestProperties clientRequestProperties = default;

        [TestCase]
        public async Task ExecuteMonitoredQueryAsync_WithValidInput_ReturnsReaderAndTime()
        {
            Metrics metric = Substitute.For<Metrics>();
            metric.AdxQueryDurationMetric = new Histogram(Substitute.For<Prometheus.IHistogram>(), "name", "help");
            client.ExecuteQueryAsync(string.Empty, Arg.Any<string>(), Arg.Any<ClientRequestProperties>()).Returns(Task.FromResult(data));
            var (timeTaken, reader) = await client.ExecuteMonitoredQueryAsync("wibble", clientRequestProperties, metric);

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(data, reader);
        }

        [TestCase]
        public void ExecuteMonitoredQueryAsync_WithNullClient_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("client cannot be null (Parameter 'client')"),
                async () => await CslQueryProviderExtensions.ExecuteMonitoredQueryAsync(null, "some query", clientRequestProperties, metric));
        }

        [TestCase]
        public void ExecuteMonitoredQueryAsync_WithEmptyQuery_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("query cannot be empty (Parameter 'query')"),
                async () => await client.ExecuteMonitoredQueryAsync(string.Empty, clientRequestProperties, metric));
        }

        [TestCase]
        public void ExecuteMonitoredQuery_WithNullQuery_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("query cannot be null (Parameter 'query')"),
                async () => await client.ExecuteMonitoredQueryAsync(null, clientRequestProperties, metric));
        }
    }
}
