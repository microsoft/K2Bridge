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
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using static System.StringComparison;

    public class KustoDataAccessTests
    {
        private static readonly object[] IndexNames = {
            new string[] { "databaseName:tableName", "databaseName", "tableName" },
            new string[] { "*", "*", "*" },
            new string[] { "tableName", string.Empty, "tableName" },
            new string[] { ":", string.Empty, string.Empty },
            new string[] { "tableName", "databaseName", "tableName" },
        };

        private Mock<IQueryExecutor> mockQueryExecutor;

        private Mock<IConnectionDetails> mockDetails;

        private IMemoryCache memoryCache;

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
            memoryCache = GetMemoryCache();
        }

        [Test]
        public async Task GetFieldCaps_WithValidIndex_ReturnFieldCaps()
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
                column("myguid", "System.Guid"),
                column("mytimespan", "System.TimeSpan"),
                column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse(@"{""a"": ""int"", ""b"": ""string"", ""c"": {""d"": {""e"": ""string""}}}") },
                },
            });

            // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
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
                          ""metadata_field"": false,
                          ""type"": ""boolean""
                        }
                      },
                      ""myint"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""integer""
                        }
                      },
                      ""mylong"": {
                        ""long"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""long""
                        }
                      },
                      ""myreal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""double""
                        }
                      },
                      ""mystring"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydatetime"": {
                        ""date"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""date""
                        }
                      },
                      ""mydynamic"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.a"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""integer""
                        }
                      },
                      ""mydynamic.b"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydynamic.c"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.c.d"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.c.d.e"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""myguid"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""string""
                        }
                      },
                      ""mytimespan"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""string""
                        }
                      },
                      ""mydecimal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""double""
                        }
                      }
                    }
                  }
                  "));

            calls[0].QueryCommandText.Should().Be("['testIndexName'] | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithValidIndex_WithCache_ReturnFieldCaps()
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
                column("myguid", "System.Guid"),
                column("mytimespan", "System.TimeSpan"),
                column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(() =>
                {
                    IDataReader testReader = new DataReaderMock(testData);
                    return Task.FromResult(testReader);
                });

            // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(() =>
                {
                    IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                        new Dictionary<string, object>() {
                            { "result", JToken.Parse(@"{""a"": ""int"", ""b"": ""string"", ""c"": {""d"": {""e"": ""string""}}}") },
                        },
                    });
                    return Task.FromResult((TimeSpan.Zero, dynamicResultReader));
                });

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);

            memoryCache.Get<FieldCapabilityResponse>("testIndexName").Should().BeNull();

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
                          ""metadata_field"": false,
                          ""type"": ""boolean""
                        }
                      },
                      ""myint"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""integer""
                        }
                      },
                      ""mylong"": {
                        ""long"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""long""
                        }
                      },
                      ""myreal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""double""
                        }
                      },
                      ""mystring"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydatetime"": {
                        ""date"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""date""
                        }
                      },
                      ""mydynamic"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.a"": {
                        ""integer"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""integer""
                        }
                      },
                      ""mydynamic.b"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""mydynamic.c"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.c.d"": {
                        ""object"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""object""
                        }
                      },
                      ""mydynamic.c.d.e"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""keyword""
                        }
                      },
                      ""myguid"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""string""
                        }
                      },
                      ""mytimespan"": {
                        ""string"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""string""
                        }
                      },
                      ""mydecimal"": {
                        ""double"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
                          ""type"": ""double""
                        }
                      }
                    }
                  }
                  "));

            calls.Should().HaveCount(1);
            calls[0].QueryCommandText.Should().Be("['testIndexName'] | summarize buildschema(mydynamic)");

            memoryCache.Get<FieldCapabilityResponse>("testIndexName").Should().NotBeNull();
            var response2 = await kusto.GetFieldCapsAsync("testIndexName");
            response2.Should().BeEquivalentTo(response);
            calls.Should().HaveCount(1);

            var response3 = await kusto.GetFieldCapsAsync("testIndexName", invalidateCache: true);
            response3.Should().BeEquivalentTo(response);
            calls.Should().HaveCount(2);
        }

        [Test]
        public async Task GetFieldCaps_WithDynamicColumnArray_ReturnFieldCaps()
        {
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("myint", "System.Int32"),
                column("nested_dynamic", "System.Object"),
                column("nested_indexer", "System.Object"),
                column("dynamic_top_level_string", "System.Object"),
                column("dynamic_top_level_indexer", "System.Object"),
                column("dynamic_top_level_array", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>()))
                .Returns((QueryData query, RequestContext context) =>
                {
                    string response;
                    if (query.QueryCommandText.Contains("nested_indexer"))
                    {
                        response = "{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": {\"b1\": \"int\", \"b2\": \"string\"}}}";
                    }
                    else if (query.QueryCommandText.Contains("nested_dynamic"))
                    {
                        response = "{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}";
                    }
                    else if (query.QueryCommandText.Contains("dynamic_top_level_string"))
                    {
                        response = "\"string\"";
                    }
                    else if (query.QueryCommandText.Contains("dynamic_top_level_indexer"))
                    {
                        response = "{\"`indexer`\": \"int\"}";
                    }
                    else
                    {
                        response = "[\"int\", \"string\"]";
                    }

                    IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                        new Dictionary<string, object>() {
                            { "result", JToken.Parse(response) },
                        },
                    });

                    return Task.FromResult((TimeSpan.Zero, dynamicResultReader));
                });

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            JToken.FromObject(response).Should().BeEquivalentTo(JToken.Parse(@"{
                      ""indices"": [
                        ""testIndexName""
                      ],
                      ""fields"": {
                        ""myint"": {
                          ""integer"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""integer""
                          }
                        },
                        ""nested_indexer"": {
                          ""object"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""object""
                          }
                        },
                        ""nested_indexer.a"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        },
                        ""nested_indexer.b"": {
                          ""object"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""object""
                          }
                        },
                        ""nested_indexer.b.b1"": {
                          ""integer"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""integer""
                          }
                        },
                        ""nested_indexer.b.b2"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        },
                        ""nested_dynamic"": {
                          ""object"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""object""
                          }
                        },
                        ""nested_dynamic.a"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        },
                        ""nested_dynamic.b"": {
                          ""integer"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""integer""
                          }
                        },
                        ""nested_dynamic.c"": {
                          ""object"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""object""
                          }
                        },
                        ""nested_dynamic.c.d"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        },
                        ""dynamic_top_level_string"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        },
                        ""dynamic_top_level_indexer"": {
                          ""integer"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""integer""
                          }
                        },
                        ""dynamic_top_level_array"": {
                          ""keyword"": {
                            ""aggregatable"": true,
                            ""searchable"": true,
                            ""metadata_field"": false,
                            ""type"": ""keyword""
                          }
                        }
                      }
                    }
                  "));
        }

        [Test]
        public async Task GetFieldCaps_WithMaxSamples_ReturnCorrectQuery()
        {
            const ulong samples = 10;
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("myint", "System.Int32"),
                column("mydynamic", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

            // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, samples);
            await kusto.GetFieldCapsAsync("testIndexName");

            calls[0].QueryCommandText.Should().Be(@"['testIndexName'] | sample 10 | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithMaxHours_ReturnCorrectQuery()
        {
            const ulong hours = 4;
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("myint", "System.Int32"),
                column("mydynamic", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

            // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, maxDynamicSamplesIngestionTimeHours: hours);
            await kusto.GetFieldCapsAsync("testIndexName");

            calls[0].QueryCommandText.Should().Be(@"['testIndexName'] | where ingestion_time() > ago(4h) | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithMaxSamplesAndMaxHours_ReturnCorrectQuery()
        {
            const ulong hours = 4;
            const ulong samples = 10;
            Func<string, string, Dictionary<string, object>> column = (name, type) =>
                new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };

            var testData = new List<Dictionary<string, object>>() {
                column("myint", "System.Int32"),
                column("mydynamic", "System.Object"),
            };
            using IDataReader testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(testReader));

            using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

            // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
            var calls = new List<QueryData>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, samples, hours);
            await kusto.GetFieldCapsAsync("testIndexName");

            calls[0].QueryCommandText.Should().Be(@"['testIndexName'] | where ingestion_time() > ago(4h) | sample 10 | summarize buildschema(mydynamic)");
        }

        [Test]
        public async Task GetFieldCaps_WithValidFunction_ReturnFieldCaps()
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
            using var testReader = new DataReaderMock(testData);
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult((TimeSpan.FromSeconds(1), (IDataReader)testReader)));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var response = await kusto.GetFieldCapsAsync("testIndexName");

            mockQueryExecutor.Verify(exec => exec.ExecuteQueryAsync(
                It.Is<QueryData>(d =>
                    d.IndexName == "testIndexName"
                    && d.QueryCommandText == "['testIndexName'] | getschema | project ColumnName, ColumnType=DataType"), It.IsAny<RequestContext>()));

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
                          ""metadata_field"": false,
                          ""type"": ""integer""
                        }
                      },
                      ""mystring"": {
                        ""keyword"": {
                          ""aggregatable"": true,
                          ""searchable"": true,
                          ""metadata_field"": false,
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
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
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
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
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
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "myTable" },
                    },
                });
            using IDataReader stubIndexReader2 = new DataReaderMock(
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "myFunction" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader1));
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                    It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader2));

            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
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
                new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
                });
            mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.Is<string>(s => s.Contains(searchString, OrdinalIgnoreCase)), It.IsAny<RequestContext>()))
                .Returns(Task.FromResult(stubIndexReader));
            var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
            var indexResponse = await kusto.ResolveIndexAsync(indexName);

            Assert.IsNotNull(indexResponse, $"null response for indexname {indexName}");
            var itr = indexResponse.Indices.GetEnumerator();
            itr.MoveNext();
            Assert.NotNull(itr.Current, $"failed to provide valid search term with database name {databaseName} and table name {tableName} from indexname {indexName}. expected: {searchString}");
        }

        private static IMemoryCache GetMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IMemoryCache>();
        }
    }
}
