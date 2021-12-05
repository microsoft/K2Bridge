// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.KustoDAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Json;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using static System.StringComparison;

    public class KustoDataAccessTests
    {
        private static readonly object[] IndexNames =
        {
            new string[] { "databaseName:tableName", "databaseName", "tableName" },
            new string[] { "*", "*", "*" },
            new string[] { "tableName", string.Empty, "tableName" },
            new string[] { ":", string.Empty, string.Empty },
            new string[] { "tableName", "databaseName", "tableName" },
        };

        private Mock<IQueryExecutor> mockQueryExecutor;

        private Mock<IConnectionDetails> mockDetails;

        [SetUp]
        public void SetUp_DefaultMocks()
        {
            mockQueryExecutor = new Mock<IQueryExecutor>();
            mockDetails = new Mock<IConnectionDetails>();
            mockDetails.SetupGet(d => d.DefaultDatabaseName).Returns(string.Empty);
            mockQueryExecutor.SetupGet(x => x.DefaultDatabaseName).Returns(mockDetails.Object.DefaultDatabaseName);
            using IDataReader emptyReader = new DataReaderMock(new List<Dictionary<string, object>>());
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(emptyReader));
            using IDataReader emptyReader2 = new DataReaderMock(new List<Dictionary<string, object>>());
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsNotNull<QueryData>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.FromSeconds(1), emptyReader2)));
        }

        [Test]
        public async Task GetFieldCaps_WithValidIndex_ReturnFieldCaps()
        {
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object>
                {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>()
            {
                column("mybool", "System.SByte"),
                column("myint", "System.Int32"),
                column("mylong", "System.Int64"),
                column("myreal", "System.Double"),
                column("mystring", "System.String"),
                column("mydatetime", "System.DateTime"),
                column("mydynamic", "System.Object"),
                column("myguid", "System.Guid"),
                column("mytimespan", "System.TimeSpan"),
                column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "result", JToken.Parse(@"{""a"": ""int"", ""b"": ""string"", ""c"": {""d"": {""e"": ""string""}}}") },
                },
            });

            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"
                  {
                    ""indices"": [
                      ""testIndexName""
                    ],
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
                      },
                      ""mydynamic.a"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""integer""
                        }
                      },
                      ""mydynamic.b"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydynamic.c.d.e"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      },
                      ""myguid"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""string""
                        }
                      },
                      ""mytimespan"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""string""
                        }
                      },
                      ""mydecimal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""double""
                        }
                      }
                    }
                  }
                  "));

            calls[0].QueryCommandText.Should().Be("testIndexName | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithDynamicColumnArray_ReturnFieldCaps()
        {
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object>
                {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>()
            {
                column("myint", "System.Int32"),
                column("mydynamic", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.Is<QueryData>(q => q.QueryCommandText.Contains("mydynamic")), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"
                  {
                    ""indices"": [
                      ""testIndexName""
                    ],
                    ""fields"": {
                      ""myint"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""integer""
                        }
                      },
                      ""mydynamic"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.a"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydynamic.b"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""integer""
                        }
                      },
                      ""mydynamic.c.d"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      }
                    }
                  }
                  "));
        }

        [Test]
        public async Task GetFieldCaps_WithPercentage_ReturnCorrectQuery()
        {
            const double percentage = 30.5;
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object>
                {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>()
            {
                column("myint", "System.Int32"),
                column("mydynamic", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, percentage);
            await kusto.GetFieldCapsAsync("testIndexName");

            calls[0].QueryCommandText.Should().Be(@"let percentage = 30.5 / 100.0;
let table_count = toscalar(testIndexName | count);
testIndexName | sample toint(floor(table_count * percentage, 1)) | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithValidFunction_ReturnFieldCaps()
        {
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object>
                {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>()
            {
                column("myint", "System.Int32"),
                column("mystring", "System.String"),
            };
            using var testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.FromSeconds(1), (IDataReader)testReader)));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            mockQueryExecutor.Verify(exec => exec.ExecuteQueryAsync(
                It.Is<QueryData>(d =>
                    d.IndexName == "testIndexName"
                    && d.QueryCommandText == "testIndexName | getschema | project ColumnName, ColumnType=DataType"), It.IsAny<RequestContext>()));

            JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"
                  {
                    ""indices"": [
                      ""testIndexName""
                    ],
                    ""fields"": {
                      ""myint"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""integer""
                        }
                      },
                      ""mystring"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""type"": ""keyword""
                        }
                      }
                    }
                  }
                  "));
        }

        [Test]
        public async Task ResolveIndex_WithValidIndex_ReturnIndexList()
        {
            using IDataReader stubIndexReader = new DataReaderMock(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>
                    {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.ResolveIndexAsync("testIndex");

            mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(
                ".show databases schema"
                + " | where TableName != ''"
                + " | distinct TableName, DatabaseName"
                + " | search TableName: 'testIndex'"
                + " | search DatabaseName: ''"
                + " |  project strcat(DatabaseName, \":\", TableName)", It.IsAny<RequestContext>()));

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Indices.GetEnumerator();
            itr.MoveNext();
            Assert.AreEqual(itr.Current.Name, "somevalue1");
            Assert.False(itr.MoveNext());
        }

        [Test]
        public async Task ResolveIndex_WithValidFunction_ReturnIndexList()
        {
            using IDataReader stubIndexReader = new DataReaderMock(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>
                    {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.ResolveIndexAsync("testIndex");

            mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(
                ".show functions"
                + " | where Parameters == '()'"
                + " | distinct Name"
                + " | search Name: 'testIndex'"
                + " | project strcat(\"\", \":\", Name)", It.IsAny<RequestContext>()));

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Indices.GetEnumerator();
            itr.MoveNext();
            Assert.AreEqual(itr.Current.Name, "somevalue1");
            Assert.False(itr.MoveNext());
        }

        [Test]
        public async Task ResolveIndex_ValidIndexAndValidFunction_ReturnBoth()
        {
            using IDataReader stubIndexReader1 = new DataReaderMock(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>
                    {
                        { "1", "myTable" },
                    },
                });
            using IDataReader stubIndexReader2 = new DataReaderMock(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>
                    {
                        { "1", "myFunction" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader1));
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader2));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.ResolveIndexAsync("testIndex");

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Indices.GetEnumerator();
            Assert.True(itr.MoveNext());
            Assert.AreEqual(itr.Current.Name, "myTable");
            Assert.True(itr.MoveNext());
            Assert.AreEqual(itr.Current.Name, "myFunction");
            Assert.False(itr.MoveNext());
        }

        [TestCaseSource(nameof(IndexNames))]
        public async Task ResolveIndex_WithValidInput_ReadsQueryExecutorValidDatabase(string indexName, string databaseName, string tableName)
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            var mockDetails = new Mock<IConnectionDetails>();
            mockDetails.SetupGet(d => d.DefaultDatabaseName).Returns(databaseName);
            mockQueryExecutor.SetupGet(x => x.DefaultDatabaseName).Returns(mockDetails.Object.DefaultDatabaseName);
            var searchString = $"search TableName: '{tableName}' | search DatabaseName: '{databaseName}' |";
            using IDataReader stubIndexReader = new DataReaderMock(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>
                    {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.Is<string>(s => s.Contains(searchString, StringComparison.OrdinalIgnoreCase)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.ResolveIndexAsync(indexName);

            Assert.IsNotNull(indexResponse, $"null response for indexname {indexName}");
            var itr = indexResponse.Indices.GetEnumerator();
            itr.MoveNext();
            Assert.NotNull(itr.Current, $"failed to provide valid search term with database name {databaseName} and table name {tableName} from indexname {indexName}. expected: {searchString}");
        }
    }
}