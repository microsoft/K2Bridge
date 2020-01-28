// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests.KustoConnector
{
    using System;
    using System.Data;
    using K2Bridge.KustoConnector;
    using Kusto.Data.Common;
    using NSubstitute;
    using NUnit.Framework;
    using Prometheus;

    [TestFixture]
    public class CslQueryProviderExtensionsTests
    {
        [TestCase]
        public void ExecuteMonitoredQuery_Common_Success()
        {
            var data = Substitute.For<IDataReader>();
            var provider = Substitute.For<ICslQueryProvider>();
            var metric = Substitute.For<IHistogram>();

            provider.ExecuteQuery(Arg.Any<string>()).Returns(data);
            var (timeTaken, reader) = provider.ExecuteMonitoredQuery("wibble", metric);

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(data, reader);
        }

        [TestCase]
        public void ExecuteMonitoredQuery_NullProvider_Failure()
        {
            var data = Substitute.For<IDataReader>();
            var metric = Substitute.For<IHistogram>();

            _ = Assert.Throws(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("Value cannot be null. (Parameter 'Argument client cannot be null')"),
                () => CslQueryProviderExtensions.ExecuteMonitoredQuery(null, "some query", metric));
        }

        [TestCase]
        public void ExecuteMonitoredQuery_EmptyQuery_Failure()
        {
            var data = Substitute.For<IDataReader>();
            var metric = Substitute.For<IHistogram>();
            var client = Substitute.For<ICslQueryProvider>();

            _ = Assert.Throws(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("query (Parameter 'query cannot be empty')"),
                () => CslQueryProviderExtensions.ExecuteMonitoredQuery(client, string.Empty, metric));
        }

        [TestCase]
        public void ExecuteMonitoredQuery_NullQuery_Failure()
        {
            var data = Substitute.For<IDataReader>();
            var metric = Substitute.For<IHistogram>();
            var client = Substitute.For<ICslQueryProvider>();

            _ = Assert.Throws(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("query cannot be null (Parameter 'query')"),
                () => CslQueryProviderExtensions.ExecuteMonitoredQuery(client, null, metric));
        }
    }
}
