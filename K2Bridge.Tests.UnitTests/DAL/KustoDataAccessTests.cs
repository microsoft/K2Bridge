// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Tests.UnitTests.DAL
{
    using Moq;
    using K2Bridge.DAL;
    using K2Bridge.KustoConnector;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using K2Bridge.Models.Response;
    using global::Tests;

    public class KustoDataAccessTests
    {
        public KustoDataAccessTests()
        {
        }

        [Test]
        public void WhenGetFieldCapsWithValidIndexReturnFieldCaps()
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommand(It.IsNotNull<string>()))
                .Returns(new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"testName", "somevalue1"},
                        {"type", "System.Int32"}
                    }
                }));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var response = kusto.GetFieldCaps("testIndexName");

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Fields);
            Assert.AreEqual(response.Fields["somevalue1"].Name, "somevalue1");
        }

        [Test]
        public void WhenGetIndexListWithValidIndexReturnFieldCaps()
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommand(It.IsNotNull<string>()))
                .Returns(new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"1", "somevalue1"}
                    }
                }));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var response = kusto.GetIndexList("testIndex");

            Assert.IsNotNull(response);
            var itr = response.Aggregations.IndexCollection.Buckets.GetEnumerator();
            itr.MoveNext();
            Assert.AreEqual(((TermBucket)itr.Current).Key, "somevalue1");
        }
    }
}
