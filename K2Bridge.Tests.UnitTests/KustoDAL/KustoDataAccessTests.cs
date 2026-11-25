// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.KustoDAL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Json;
using K2Bridge.KustoDAL;
using K2Bridge.Models;
using K2Bridge.Models.Response.Metadata;
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
            new string[] { "logs*", "defaultdb", "logs*" },
            new string[] { "prod*:logs*", "prod*", "logs*" },
            new string[] { "prod-db:logs.2026-11", "prod-db", "logs.2026-11" },
            new string[] { "prod db:logs archive", "prod db", "logs archive" },
            new string[] { "defaultdb:logs:archive", "defaultdb", "logs:archive" },
            new string[] { "*:logs*", "*", "logs*" },
            new string[] { "prod*:*", "prod*", "*" },
            new string[] { "*logs*", "defaultdb", "*logs*" },
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
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(emptyReader));
        using IDataReader emptyReader2 = new DataReaderMock(new List<Dictionary<string, object>>());
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsNotNull<QueryData>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult((TimeSpan.FromSeconds(1), emptyReader2)));
        memoryCache = GetMemoryCache();
    }

    [Test]
    public async Task GetFieldCaps_WithValidIndex_ReturnFieldCaps()
    {
        var testData = new List<Dictionary<string, object>>() {
                Column("mybool", "System.SByte"),
                Column("myint", "System.Int32"),
                Column("mylong", "System.Int64"),
                Column("myreal", "System.Double"),
                Column("mystring", "System.String"),
                Column("mydatetime", "System.DateTime"),
                Column("mydynamic", "System.Object"),
                Column("myguid", "System.Guid"),
                Column("mytimespan", "System.TimeSpan"),
                Column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
        using IDataReader testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(testReader));

        using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse(@"{""a"": ""int"", ""b"": ""string"", ""c"": {""d"": {""e"": ""string""}}}") },
                },
            });

        // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
        var calls = new List<QueryData>();
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>(), string.Empty))
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
        var testData = new List<Dictionary<string, object>>() {
                Column("mybool", "System.SByte"),
                Column("myint", "System.Int32"),
                Column("mylong", "System.Int64"),
                Column("myreal", "System.Double"),
                Column("mystring", "System.String"),
                Column("mydatetime", "System.DateTime"),
                Column("mydynamic", "System.Object"),
                Column("myguid", "System.Guid"),
                Column("mytimespan", "System.TimeSpan"),
                Column("mydecimal", "System.Data.SqlTypes.SqlDecimal"),
            };
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(() =>
            {
                IDataReader testReader = new DataReaderMock(testData);
                return Task.FromResult(testReader);
            });

        // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
        var calls = new List<QueryData>();
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>(), string.Empty))
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
        var testData = new List<Dictionary<string, object>>() {
                Column("myint", "System.Int32"),
                Column("nested_dynamic", "System.Object"),
                Column("nested_indexer", "System.Object"),
                Column("dynamic_top_level_string", "System.Object"),
                Column("dynamic_top_level_indexer", "System.Object"),
                Column("dynamic_top_level_array", "System.Object"),
            };
        using IDataReader testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(testReader));

        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns((QueryData query, RequestContext context, string db) =>
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

        var testData = new List<Dictionary<string, object>>() {
                Column("myint", "System.Int32"),
                Column("mydynamic", "System.Object"),
            };
        using IDataReader testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(testReader));

        using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

        // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
        var calls = new List<QueryData>();
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, samples);
        await kusto.GetFieldCapsAsync("testIndexName");

        calls[0].QueryCommandText.Should().Be(@"['testIndexName'] | sample 10 | summarize buildschema(mydynamic)");
    }

    [Test]
    public async Task GetFieldCaps_WithMaxHours_ReturnCorrectQuery()
    {
        const ulong hours = 4;

        var testData = new List<Dictionary<string, object>>() {
                Column("myint", "System.Int32"),
                Column("mydynamic", "System.Object"),
            };
        using IDataReader testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(testReader));

        using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

        // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
        var calls = new List<QueryData>();
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>(), string.Empty))
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

        var testData = new List<Dictionary<string, object>>() {
                Column("myint", "System.Int32"),
                Column("mydynamic", "System.Object"),
            };
        using IDataReader testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(testReader));

        using IDataReader dynamicResultReader = new DataReaderMock(new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "result", JToken.Parse("{\"a\": [\"int\", \"string\"], \"b\": {\"`indexer`\": \"int\"}, \"c\": {\"d\": [{\"e\": \"string\"}, \"int\"]}}") },
                },
            });

        // We capture the calls to ExecuteQueryAsync to verify it calls the correct query to build dynamic fields
        var calls = new List<QueryData>();
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(Capture.In(calls), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult((TimeSpan.Zero, dynamicResultReader)));

        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object, samples, hours);
        await kusto.GetFieldCapsAsync("testIndexName");

        calls[0].QueryCommandText.Should().Be(@"['testIndexName'] | where ingestion_time() > ago(4h) | sample 10 | summarize buildschema(mydynamic)");
    }

    [Test]
    public async Task GetFieldCaps_WithValidFunction_ReturnFieldCaps()
    {
        var testData = new List<Dictionary<string, object>>() {
                Column("myint", "System.Int32"),
                Column("mystring", "System.String"),
            };
        using var testReader = new DataReaderMock(testData);
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult((TimeSpan.FromSeconds(1), (IDataReader)testReader)));

        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
        var response = await kusto.GetFieldCapsAsync("testIndexName");

        mockQueryExecutor.Verify(exec => exec.ExecuteQueryAsync(
            It.Is<QueryData>(d =>
                d.IndexName == "testIndexName"
                && d.QueryCommandText == "['testIndexName'] | getschema | project ColumnName, ColumnType=DataType"),
            It.IsAny<RequestContext>(),
            string.Empty));

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
                It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(stubIndexReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
        var indexResponse = await kusto.ResolveIndexAsync("testIndex");

        mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(
            ".show databases schema"
            + " | where TableName != ''"
            + " | distinct TableName, DatabaseName"
            + " | search TableName: \"testIndex\""
            + " | search DatabaseName: \"\""
            + " |  project strcat(DatabaseName, \":\", TableName)",
            It.IsAny<RequestContext>(),
            string.Empty));

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
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(stubIndexReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
        var indexResponse = await kusto.ResolveIndexAsync("testIndex");

        mockQueryExecutor.Verify(exec => exec.ExecuteControlCommandAsync(
            ".show functions"
            + " | where Parameters == '()'"
            + " | distinct Name"
            + " | search Name: \"testIndex\""
            + " | project strcat(\"\", \":\", Name)",
            It.IsAny<RequestContext>(),
            string.Empty));

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
                It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(stubIndexReader1));
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
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
        var searchString = $"search TableName: \"{tableName}\" | search DatabaseName: \"{databaseName}\" |";
        using IDataReader stubIndexReader = new DataReaderMock(
            new List<Dictionary<string, object>>() {
                    new Dictionary<string, object> {
                        { "1", "somevalue1" },
                    },
            });
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(It.Is<string>(s => s.Contains(searchString, OrdinalIgnoreCase)), It.IsAny<RequestContext>(), string.Empty))
            .Returns(Task.FromResult(stubIndexReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, It.IsAny<RequestContext>(), new Mock<ILogger<KustoDataAccess>>().Object);
        var indexResponse = await kusto.ResolveIndexAsync(indexName);

        Assert.IsNotNull(indexResponse, $"null response for indexname {indexName}");
        var itr = indexResponse.Indices.GetEnumerator();
        itr.MoveNext();
        Assert.NotNull(itr.Current, $"failed to provide valid search term with database name {databaseName} and table name {tableName} from indexname {indexName}. expected: {searchString}");
    }

    [Test]
    public async Task GetFieldCaps_WithIndexInjection_EncodesSchemaAndFunctionQueries()
    {
        const string tableName = "logs'] | union database(\"other\").['secret";
        const string indexName = "defaultdb:" + tableName;
        using IDataReader schemaReader = new DataReaderMock(new List<Dictionary<string, object>>());
        using IDataReader functionReader = new DataReaderMock(new List<Dictionary<string, object>>());
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.IsAny<string>(), It.IsAny<RequestContext>(), "defaultdb"))
            .ReturnsAsync(schemaReader);
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(
                It.IsAny<QueryData>(), It.IsAny<RequestContext>(), "defaultdb"))
            .ReturnsAsync((TimeSpan.Zero, functionReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, null, new Mock<ILogger<KustoDataAccess>>().Object);

        await kusto.GetFieldCapsAsync(indexName);

        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show databases schema | where TableName==\"logs'] | union database(\\\"other\\\").['secret\""
                + " and DatabaseName==\"defaultdb\" and ColumnName!='' | project ColumnName, ColumnType",
                It.IsAny<RequestContext>(),
                "defaultdb"),
            Times.Once);
        mockQueryExecutor.Verify(
            exec => exec.ExecuteQueryAsync(
                It.Is<QueryData>(q => q.QueryCommandText == "['logs\\'] | union database(\"other\").[\\'secret']"
                    + " | getschema | project ColumnName, ColumnType=DataType"),
                It.IsAny<RequestContext>(),
                "defaultdb"),
            Times.Once);
    }

    [Test]
    public async Task GetTablesAndFunctions_WithQuotedIndex_EncodesSearchLiterals()
    {
        const string databaseName = "db'\\name";
        const string tableName = "logs'] | union database(\"other\").['secret";
        using IDataReader tablesReader = new DataReaderMock(new List<Dictionary<string, object>>());
        using IDataReader functionsReader = new DataReaderMock(new List<Dictionary<string, object>>());
        mockQueryExecutor.SetupGet(exec => exec.DefaultDatabaseName).Returns(databaseName);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .ReturnsAsync(tablesReader);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .ReturnsAsync(functionsReader);
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, null, new Mock<ILogger<KustoDataAccess>>().Object);

        await kusto.GetTablesAndFunctions(databaseName + ":" + tableName);

        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show databases schema | where TableName != '' | distinct TableName, DatabaseName"
                + " | search TableName: \"logs'] | union database(\\\"other\\\").['secret\""
                + " | search DatabaseName: \"db'\\\\name\" |  project strcat(DatabaseName, \":\", TableName)",
                It.IsAny<RequestContext>(),
                string.Empty),
            Times.Once);
        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show functions | where Parameters == '()' | distinct Name"
                + " | search Name: \"logs'] | union database(\\\"other\\\").['secret\""
                + " | project strcat(\"db'\\\\name\", \":\", Name)",
                It.IsAny<RequestContext>(),
                string.Empty),
            Times.Once);
    }

    [TestCaseSource(nameof(EncodedNameTestCases))]
    public async Task GetFieldCaps_WithEncodedNames_PreservesFunctionSchema(
        string databaseName, string tableName, string databaseLiteral, string tableLiteral, string tableIdentifier)
    {
        using IDataReader schemaReader = new DataReaderMock(new List<Dictionary<string, object>>());
        using IDataReader functionReader = new DataReaderMock(new List<Dictionary<string, object>>
        {
            Column("message", "System.String"),
        });
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.IsAny<string>(), It.IsAny<RequestContext>(), databaseName))
            .ReturnsAsync(schemaReader);
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(
                It.IsAny<QueryData>(), It.IsAny<RequestContext>(), databaseName))
            .ReturnsAsync((TimeSpan.Zero, functionReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, null, new Mock<ILogger<KustoDataAccess>>().Object);

        var response = await kusto.GetFieldCapsAsync(databaseName + ":" + tableName);

        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show databases schema | where TableName==" + tableLiteral
                + " and DatabaseName==" + databaseLiteral
                + " and ColumnName!='' | project ColumnName, ColumnType",
                It.IsAny<RequestContext>(),
                databaseName),
            Times.Once);
        mockQueryExecutor.Verify(
            exec => exec.ExecuteQueryAsync(
                It.Is<QueryData>(q => q.IndexName == tableName
                    && q.QueryCommandText == tableIdentifier
                    + " | getschema | project ColumnName, ColumnType=DataType"),
                It.IsAny<RequestContext>(),
                databaseName),
            Times.Once);
        Assert.That(response.Indices, Is.EqualTo(new[] { tableName }));
        Assert.That(response.Fields.ContainsKey("message"), Is.True);
    }

    [TestCaseSource(nameof(EncodedNameTestCases))]
    public async Task GetTablesAndFunctions_WithEncodedNames_PreservesReturnedIndices(
        string databaseName, string tableName, string databaseLiteral, string tableLiteral, string _)
    {
        const string defaultDatabase = "configured\"db\\";
        const string defaultDatabaseLiteral = "\"configured\\\"db\\\\\"";
        using IDataReader tablesReader = new DataReaderMock(new List<Dictionary<string, object>>
        {
            new() { { "Name", databaseName + ":" + tableName } },
        });
        using IDataReader functionsReader = new DataReaderMock(new List<Dictionary<string, object>>
        {
            new() { { "Name", defaultDatabase + ":function" } },
        });
        mockQueryExecutor.SetupGet(exec => exec.DefaultDatabaseName).Returns(defaultDatabase);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show databases", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .ReturnsAsync(tablesReader);
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.Is<string>(q => q.StartsWith(".show functions", Ordinal)), It.IsAny<RequestContext>(), string.Empty))
            .ReturnsAsync(functionsReader);
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, null, new Mock<ILogger<KustoDataAccess>>().Object);

        var indices = await kusto.GetTablesAndFunctions(databaseName + ":" + tableName);

        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show databases schema | where TableName != '' | distinct TableName, DatabaseName"
                + " | search TableName: " + tableLiteral + " | search DatabaseName: " + databaseLiteral
                + " |  project strcat(DatabaseName, \":\", TableName)",
                It.IsAny<RequestContext>(),
                string.Empty),
            Times.Once);
        mockQueryExecutor.Verify(
            exec => exec.ExecuteControlCommandAsync(
                ".show functions | where Parameters == '()' | distinct Name"
                + " | search Name: " + tableLiteral
                + " | project strcat(" + defaultDatabaseLiteral + ", \":\", Name)",
                It.IsAny<RequestContext>(),
                string.Empty),
            Times.Once);
        Assert.That(indices, Is.EqualTo(new[] { databaseName + ":" + tableName, defaultDatabase + ":function" }));
    }

    [TestCase("logs archive", "['logs archive']")]
    [TestCase("logs'\\", "['logs\\'\\\\']")]
    [TestCase("logs\nnext", "['logs\\u000anext']")]
    public async Task GetFieldCaps_WithQuotedTable_PreservesDynamicSchema(string tableName, string tableIdentifier)
    {
        using IDataReader schemaReader = new DataReaderMock(new List<Dictionary<string, object>>
        {
            Column("properties", "System.Object"),
        });
        using IDataReader dynamicReader = new DataReaderMock(new List<Dictionary<string, object>>
        {
            new() { { "result", JToken.Parse(@"{""value"": ""long""}") } },
        });
        mockQueryExecutor.Setup(exec => exec.ExecuteControlCommandAsync(
                It.IsAny<string>(), It.IsAny<RequestContext>(), "defaultdb"))
            .ReturnsAsync(schemaReader);
        mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(
                It.IsAny<QueryData>(), It.IsAny<RequestContext>(), "defaultdb"))
            .ReturnsAsync((TimeSpan.Zero, dynamicReader));
        var kusto = new KustoDataAccess(memoryCache, mockQueryExecutor.Object, null, new Mock<ILogger<KustoDataAccess>>().Object, 5, 2);

        var response = await kusto.GetFieldCapsAsync("defaultdb:" + tableName);

        mockQueryExecutor.Verify(
            exec => exec.ExecuteQueryAsync(
                It.Is<QueryData>(q => q.IndexName == tableName
                    && q.QueryCommandText == tableIdentifier
                    + " | where ingestion_time() > ago(2h) | sample 5 | summarize buildschema(properties)"),
                It.IsAny<RequestContext>(),
                "defaultdb"),
            Times.Once);
        Assert.That(response.Indices, Is.EqualTo(new[] { tableName }));
        Assert.That(response.Fields.ContainsKey("properties.value"), Is.True);
    }

    private static IEnumerable<TestCaseData> EncodedNameTestCases()
    {
        yield return new TestCaseData("prod-db", "logs.2026-11", "\"prod-db\"", "\"logs.2026-11\"", "['logs.2026-11']");
        yield return new TestCaseData("prod db", "logs archive", "\"prod db\"", "\"logs archive\"", "['logs archive']");
        yield return new TestCaseData("db\"\\", "lo'gs\\", "\"db\\\"\\\\\"", "\"lo'gs\\\\\"", "['lo\\'gs\\\\']");
        yield return new TestCaseData("db\\n", "logs\\u0027", "\"db\\\\n\"", "\"logs\\\\u0027\"", "['logs\\\\u0027']");
        yield return new TestCaseData("db\nnext", "logs\r\nnext", "\"db\\u000anext\"", "\"logs\\u000d\\u000anext\"", "['logs\\u000d\\u000anext']");
        yield return new TestCaseData("db\u2028next", "logs\u2029next", "\"db\\u2028next\"", "\"logs\\u2029next\"", "['logs\\u2029next']");
        yield return new TestCaseData(string.Empty, "logs:archive", "\"\"", "\"logs:archive\"", "['logs:archive']");
        yield return new TestCaseData("\u6570\u636e", "\u65e5\u5fd7", "\"\u6570\u636e\"", "\"\u65e5\u5fd7\"", "['\u65e5\u5fd7']");
    }

    private static IMemoryCache GetMemoryCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetService<IMemoryCache>();
    }

    private static Dictionary<string, object> Column(string name, string type)
    {
        return new Dictionary<string, object> {
                    { "ColumnName", name },
                    { "ColumnType", type },
                };
    }
}
