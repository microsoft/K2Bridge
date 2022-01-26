// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Telemetry;
    using Kusto.Data.Common;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CslQueryProviderExtensionsTests
    {
        private readonly IDataReader stubReader = new Mock<IDataReader>().Object;
        private readonly Mock<ICslQueryProvider> stubClient = new Mock<ICslQueryProvider>();
        private readonly Mock<Metrics> stubMetrics = new Mock<Metrics>();
        private readonly ClientRequestProperties clientRequestProperties = default;

        [Test]
        public async Task ExecuteMonitoredQueryAsync_WithValidInput_ReturnsReaderAndTime()
        {
            var metrics = Metrics.Create(GetMockTelemetryClient());
            stubClient.Setup(client => client.ExecuteQueryAsync(string.Empty, It.IsAny<string>(), It.IsAny<ClientRequestProperties>()))
                .Returns(Task.FromResult(stubReader));
            var (timeTaken, reader) = await stubClient.Object.ExecuteMonitoredQueryAsync("wibble", clientRequestProperties, metrics);

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(reader, reader);
        }

        [Test]
        public void ExecuteMonitoredQueryAsync_WithNullClient_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("client cannot be null (Parameter 'client')"),
                async () => await CslQueryProviderExtensions.ExecuteMonitoredQueryAsync(null, "some query", clientRequestProperties, stubMetrics.Object));
        }

        [Test]
        public void ExecuteMonitoredQueryAsync_WithEmptyQuery_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("query cannot be empty (Parameter 'query')"),
                async () => await stubClient.Object.ExecuteMonitoredQueryAsync(string.Empty, clientRequestProperties, stubMetrics.Object));
        }

        [Test]
        public void ExecuteMonitoredQuery_WithNullQuery_ThrowsException()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("query cannot be null (Parameter 'query')"),
                async () => await stubClient.Object.ExecuteMonitoredQueryAsync(null, clientRequestProperties, stubMetrics.Object));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "No need to do this.")]
        private TelemetryClient GetMockTelemetryClient()
        {
            return new TelemetryClient(
                new TelemetryConfiguration
                {
                    TelemetryChannel = new Mock<ITelemetryChannel>().Object,
                });
        }
    }
}
