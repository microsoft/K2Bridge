// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests.KustoConnector
{
    using System.Data;
    using K2Bridge.KustoConnector;
    using Kusto.Data.Common;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class CslQueryProviderExtensionsTests
    {
        [TestCase]
        public void ExecutesQuery()
        {
            var data = Substitute.For<IDataReader>();
            var provider = Substitute.For<ICslQueryProvider>();

            provider.ExecuteQuery(Arg.Any<string>()).Returns(data);
            var (timeTaken, reader) = provider.ExecuteMonitoredQuery("wibble");

            Assert.AreNotEqual(0, timeTaken);
            Assert.AreSame(data, reader);
        }
    }
}
