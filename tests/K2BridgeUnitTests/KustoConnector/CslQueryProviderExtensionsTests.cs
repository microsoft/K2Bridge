using System;
using System.Data;
using K2Bridge.KustoConnector;
using Kusto.Data.Common;
using NSubstitute;
using NUnit.Framework;

namespace Tests.KustoConnector
{
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
