// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoDAL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Telemetry;
    using Kusto.Data.Common;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class KustoQueryExecutorTests
    {
        [Test]
        public async Task ExecuteControlCommandAsync_WhenCalled_ThenReturnDataReader()
        {
            // Arrange
            var queryClientMock = new Mock<ICslQueryProvider>();
            var adminClientMock = new Mock<ICslAdminProvider>();
            var loggerMock = new Mock<ILogger<KustoQueryExecutor>>().Object;
            var metricsHistograms = new Metrics();
            var expectedDataReaderMock = new Mock<IDataReader>();

            adminClientMock.Setup(client =>
                    client.ExecuteControlCommandAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ClientRequestProperties>()))
                .Returns(Task.FromResult(expectedDataReaderMock.Object));

            KustoQueryExecutor kustoQueryExecutor = new KustoQueryExecutor(queryClientMock.Object, adminClientMock.Object, loggerMock, metricsHistograms);
            RequestContext requestContext = new RequestContext { CorrelationId = Guid.Empty };

            // Act
            var result = await kustoQueryExecutor.ExecuteControlCommandAsync("test command", requestContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDataReaderMock.Object, result);
        }

        [Test]
        public void KustoQueryExecutorConstructor_WithNullQueryAndClientArguments_ThrowException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KustoQueryExecutor>>().Object;
            var metricsHistograms = new Metrics();

            Assert.Throws(typeof(ArgumentNullException), () => new KustoQueryExecutor(null, null, loggerMock, metricsHistograms));
        }

        [Test]
        public async Task ExecuteQueryAsync_WhenCalled_ThanReturnCorrectValues()
        {
            // Arrange
            var queryClientMock = new Mock<ICslQueryProvider>();
            var adminClientMock = new Mock<ICslAdminProvider>();
            var loggerMock = new Mock<ILogger<KustoQueryExecutor>>().Object;
            Metrics metricsHistograms = Metrics.Create(GetMockTelemetryClient());
            var expectedDataReaderMock = new Mock<IDataReader>();
            var queryData = new QueryData("test command text", "test index name");

            queryClientMock.Setup(client =>
                    client.ExecuteQueryAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ClientRequestProperties>()))
                .Returns(Task.FromResult(expectedDataReaderMock.Object));

            KustoQueryExecutor kustoQueryExecutor = new KustoQueryExecutor(queryClientMock.Object, adminClientMock.Object, loggerMock, metricsHistograms);
            RequestContext requestContext = new RequestContext { CorrelationId = Guid.Empty };

            // Act
            var (timeTaken, dataReader) = await kustoQueryExecutor.ExecuteQueryAsync(queryData, requestContext);

            // Assert
            Assert.IsTrue(timeTaken.Ticks > 0);
            Assert.AreEqual(expectedDataReaderMock.Object, dataReader);
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
