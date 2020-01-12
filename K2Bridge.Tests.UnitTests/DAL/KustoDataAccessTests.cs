// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.DAL
{
    using System;
    using System.Collections.Generic;
    using global::Tests;
    using FluentAssertions;
    using FluentAssertions.Json;
    using K2Bridge.DAL;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Newtonsoft.Json.Linq;

    public class KustoDataAccessTests
    {
        public KustoDataAccessTests()
        {
        }

        [Test]
        public void WhenGetFieldCapsWithValidIndexReturnFieldCaps()
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("mybool", "System.SByte"),
                column("myint", "System.Int32"),
                column("mylong", "System.Int64"),
                column("myreal", "System.Double"),
                column("mystring", "System.String"),
                column("mydatetime", "System.DateTime"),
                column("mydynamic", "System.Object"),

                // TODO add missing Kusto types
                // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1452
                // column("myguid", "System.Guid"),
                // column("mytimespan", "System.TimeSpan"),
                // column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
            using (var testReader = new TestDataReader(testData))
            {
                mockQueryExecutor.Setup(exec => exec.ExecuteControlCommand(It.IsNotNull<string>()))
                    .Returns(testReader);

                var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
                var response = kusto.GetFieldCaps("testIndexName");

                JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"
                  {
                    ""fields"": {
                      ""mybool"": {
                        ""boolean"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""boolean""
                        }
                      },
                      ""myint"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""integer""
                        }
                      },
                      ""mylong"": {
                        ""long"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""long""
                        }
                      },
                      ""myreal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""double""
                        }
                      },
                      ""mystring"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydatetime"": {
                        ""date"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""date""
                        }
                      },
                      ""mydynamic"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""object""
                        }
                      }
                    }
                  }
                  "));
            }
        }

        [Test]
        public void WhenGetIndexListWithValidIndexReturnFieldCaps()
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommand(It.IsNotNull<string>()))
                .Returns(new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
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
