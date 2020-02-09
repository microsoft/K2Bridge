﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests.KustoConnector
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using K2Bridge.KustoConnector;
    using Kusto.Data.Common;
    using NSubstitute;
    using NUnit.Framework;
    using Prometheus;

    [TestFixture]
    public class CslQueryProviderExtensionsTests
    {
        private readonly IDataReader data = Substitute.For<IDataReader>();
        private readonly ICslQueryProvider client = Substitute.For<ICslQueryProvider>();
        private readonly IHistogram metric = Substitute.For<IHistogram>();
        private readonly ClientRequestProperties clientRequestProperties = default;

        [TestCase]
        public async Task ExecuteMonitoredQuery_Common_Success()
        {
            client.ExecuteQueryAsync(string.Empty, Arg.Any<string>(), Arg.Any<ClientRequestProperties>()).Returns(Task.FromResult(data));
            var (timeTaken, reader) = await client.ExecuteMonitoredQueryAsync("wibble", clientRequestProperties, metric);

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(data, reader);
        }

        [TestCase]
        public void ExecuteMonitoredQuery_NullProvider_Failure()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("client cannot be null (Parameter 'client')"),
                async () => await CslQueryProviderExtensions.ExecuteMonitoredQueryAsync(null, "some query", clientRequestProperties, metric));
        }

        [TestCase]
        public void ExecuteMonitoredQuery_EmptyQuery_Failure()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("query cannot be empty (Parameter 'query')"),
                async () => await client.ExecuteMonitoredQueryAsync(string.Empty, clientRequestProperties, metric));
        }

        [TestCase]
        public void ExecuteMonitoredQuery_NullQuery_Failure()
        {
            Assert.ThrowsAsync(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("query cannot be null (Parameter 'query')"),
                async () => await client.ExecuteMonitoredQueryAsync(null, clientRequestProperties, metric));
        }
    }
}
