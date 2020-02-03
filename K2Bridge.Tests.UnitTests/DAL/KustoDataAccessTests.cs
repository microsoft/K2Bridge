﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Json;
    using global::Tests;
    using K2Bridge.DAL;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using static System.StringComparison;

    public class KustoDataAccessTests
    {
        private static object[] indexNames = {
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
            mockQueryExecutor.SetupGet(x => x.ConnectionDetails).Returns(mockDetails.Object);
            using IDataReader emptyReader = new TestDataReader(new List<Dictionary<string, object>>());
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>()))
                .Returns(Task.FromResult(emptyReader));
            using IDataReader emptyReader2 = new TestDataReader(new List<Dictionary<string, object>>());
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsNotNull<QueryData>()))
                .Returns(Task.FromResult((TimeSpan.FromSeconds(1), emptyReader2)));
        }

        [Test]
        public async Task When_GetFieldCaps_With_ValidIndex_Return_FieldCaps()
        {
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
            using IDataReader testReader = new TestDataReader(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>()))
                .Returns(Task.FromResult(testReader));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

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

        [Test]
        public async Task When_GetFieldCaps_With_ValidFunction_Return_FieldCaps()
        {
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("myint", "System.Int32"),
                column("mystring", "System.String"),
            };
            using var testReader = new TestDataReader(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>()))
                .Returns(Task.FromResult((TimeSpan.FromSeconds(1), (IDataReader)testReader)));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            mockQueryExecutor.Verify(exec => exec.ExecuteQueryAsync(It.Is<QueryData>(d =>
                d.IndexName == "testIndexName"
                && d.QueryCommandText == "testIndexName | getschema | project ColumnName, ColumnType=DataType")));

            JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"
                  {
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
        public async Task When_GetIndexList_With_ValidIndex_Return_IndexList()
        {
            using IDataReader stubIndexReader = new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
            It.Is<string>(q => q.StartsWith(".show databases", Ordinal))))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.GetIndexListAsync("testIndex");

            mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(
                ".show databases schema"
                + " | where TableName != ''"
                + " | distinct TableName, DatabaseName"
                + " | search TableName: 'testIndex'"
                + " | search DatabaseName: ''"
                + " |  project strcat(DatabaseName, \":\", TableName)"));

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Aggregations.IndexCollection.Buckets.GetEnumerator();
            itr.MoveNext();
            Assert.AreEqual(((TermBucket)itr.Current).Key, "somevalue1");
            Assert.False(itr.MoveNext());
        }

        [Test]
        public async Task When_GetIndexList_With_ValidFunction_Return_IndexList()
        {
            using IDataReader stubIndexReader = new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal))))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.GetIndexListAsync("testIndex");

            mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(".show functions"
                + " | where Parameters == '()'"
                + " | distinct Name"
                + " | search Name: 'testIndex'"
                + " | project strcat(\"\", \":\", Name)"));

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Aggregations.IndexCollection.Buckets.GetEnumerator();
            itr.MoveNext();
            Assert.AreEqual(((TermBucket)itr.Current).Key, "somevalue1");
            Assert.False(itr.MoveNext());
        }

        [Test]
        public async Task When_GetIndexList_With_ValidIndexAndValidFunction_Return_Both()
        {
            using IDataReader stubIndexReader1 = new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "myTable" },
                    },
                });
            using IDataReader stubIndexReader2 = new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "myFunction" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show databases", Ordinal))))
                .Returns(Task.FromResult(stubIndexReader1));
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal))))
                .Returns(Task.FromResult(stubIndexReader2));

            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.GetIndexListAsync("testIndex");

            Assert.IsNotNull(indexResponse);
            var itr = indexResponse.Aggregations.IndexCollection.Buckets.GetEnumerator();
            Assert.True(itr.MoveNext());
            Assert.AreEqual(((TermBucket)itr.Current).Key, "myTable");
            Assert.True(itr.MoveNext());
            Assert.AreEqual(((TermBucket)itr.Current).Key, "myFunction");
            Assert.False(itr.MoveNext());
        }

        [TestCaseSource("indexNames")]
        public async Task DatabaseAndTableNamesAreSetOnKustoQuery(string indexName, string databaseName, string tableName)
        {
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            var mockDetails = new Mock<IConnectionDetails>();
            mockDetails.SetupGet(d => d.DefaultDatabaseName).Returns(databaseName);
            mockQueryExecutor.SetupGet(x => x.ConnectionDetails).Returns(mockDetails.Object);
            var searchString = $"search TableName: '{tableName}' | search DatabaseName: '{databaseName}' |";
            using IDataReader stubIndexReader = new TestDataReader(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.Is<string>(s => s.Contains(searchString))))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(mockQueryExecutor.Object, new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.GetIndexListAsync(indexName);

            Assert.IsNotNull(indexResponse, $"null response for indexname {indexName}");
            var itr = indexResponse.Aggregations.IndexCollection.Buckets.GetEnumerator();
            itr.MoveNext();
            Assert.NotNull(itr.Current, $"failed to provide valid search term with database name {databaseName} and table name {tableName} from indexname {indexName}. expexted: {searchString}");
        }
    }
}
