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
        public void ExecutesQuery()
        {
            var data = Substitute.For<IDataReader>();
            var provider = Substitute.For<ICslQueryProvider>();
            var metric = Substitute.For<IHistogram>();

            provider.ExecuteQuery(Arg.Any<string>()).Returns(data);
            var (timeTaken, reader) = provider.ExecuteMonitoredQuery("wibble", metric);

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(data, reader);
        }

        /* TODO in Task 1547 (https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1547)
        [TestCase]
        public void FailsOnEmptyQueryString()
        {
            var data = Substitute.For<IDataReader>();
            var provider = Substitute.For<ICslQueryProvider>();

            provider.ExecuteQuery(Arg.Any<string>()).Returns(data);
            _ = Assert.Throws(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("Value cannot be null or empty. (Parameter 'query')"),
                () => provider.ExecuteMonitoredQuery(string.Empty));
        }

        [TestCase]
        public void FailsOnNullQueryString()
        {
            var data = Substitute.For<IDataReader>();
            var provider = Substitute.For<ICslQueryProvider>();

            provider.ExecuteQuery(Arg.Any<string>()).Returns(data);
            _ = Assert.Throws(
                Is.TypeOf<ArgumentException>()
                 .And.Message.EqualTo("Value cannot be null or empty. (Parameter 'query')"),
                () => provider.ExecuteMonitoredQuery(null));
        }
        */

        [TestCase]
        public void FailsOnNullProvider()
        {
            var data = Substitute.For<IDataReader>();
            var metric = Substitute.For<IHistogram>();

            _ = Assert.Throws(
                Is.TypeOf<ArgumentNullException>()
                 .And.Message.EqualTo("Value cannot be null. (Parameter 'client')"),
                () => CslQueryProviderExtensions.ExecuteMonitoredQuery(null, "some query", metric));
        }
    }
}
