// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.KustoDAL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions.Json;
using K2Bridge.KustoDAL;
using K2Bridge.Models;
using K2Bridge.Models.Response;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

[TestFixture]
public class HitsMapperTests
{
    private JToken expected;
    private QueryData defaultQuery;

    [SetUp]
    public void SetUp()
    {
        var expectedStr = @"[{
              ""_index"": ""myIndex"",
              ""_type"": ""_doc"",
              ""_id"": null,
              ""_version"": 1,
              ""_score"": null,
              ""_source"": {
                ""instant"": ""2017-01-02T13:04:05.06"",
                ""value"": 234,
                ""label1"": ""boxes"",
                ""label2"": ""boxes"",
                ""label3"": ""boxes of boxes""
              },
              ""fields"": {},
              ""sort"": []
            }]";
        using (var sr = new StringReader(expectedStr))
        using (var jr = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None })
        {
            expected = JToken.ReadFrom(jr);
        }

        defaultQuery = new QueryData("query", "index", null);
    }

    [Test]
    public void MapRowsToHits_WithIdInSample_ReturnsIdInHitsArray()
    {
        var table = SampleDataWith("12345");

        var hits = ReadHits(table, defaultQuery);

        // Assert
        Assert.That(hits.Single().Id, Is.EqualTo("12345"));
    }

    [Test]
    public void MapsRowsToHits_WithoutIdInSample_ReturnsSomeRandomNonEmptyArray()
    {
        var table = SampleDataWith(id: string.Empty);

        var hits = ReadHits(table, defaultQuery);

        // Assert
        Assert.That(hits.Single().Id, Is.Not.Empty);
    }

    [Test]
    public void MapRowsToHits_WithEmptyQueryAndSample_ReturnsNullHitsArray()
    {
        // Arrange
        using var table = SampleData();
        var query = new QueryData(
            "myKQL",
            "myIndex",
            highlightText: new Dictionary<string, string> { });

        // Act
        var hits = ReadHits(table, query);

        // Assert
        AssertHits(hits);
    }

    [Test]
    public void MapRowsToHits_WithHighlight_ReturnsHitsWithHighlight()
    {
        // Arrange
        using var table = SampleData();
        var query = new QueryData(
            "myKQL",
            "myIndex",
            highlightText: new Dictionary<string, string> {
                    { "*", "boxes" },
            })
        {
            HighlightPreTag = "Foo",
            HighlightPostTag = "Bar",
        };

        // Assert
        var highlight = JToken.FromObject(new string[] { "FooboxesBar" });
        expected[0]["highlight"] = new JObject
        {
            ["label1"] = highlight,
            ["label2"] = highlight,
            ["label3"] = JToken.FromObject(new string[] { "FooboxesBar of FooboxesBar" }),
        };

        // Act
        var hits = ReadHits(table, query);

        // Assert
        AssertHits(hits);
    }

    [TestCase("return of the pink panther pink", "title:*return*", "@return$ of the pink panther pink")]
    public void MapRowsToHits_WithLuceneSpecialCharsHighlight_ReturnsHitsWithHighlight(string text, string highlightString, string expectedString)
    {
        HitCreateWithHighlightsAdvancedCases(text, highlightString, expectedString);
    }

    [Test]
    [TestCase("Robert 'Bob' Kennedy", "Bob", "Robert '@Bob$' Kennedy", TestName = "MapRowsToHits_HighlightSingleTerm_ReturnsValidHighlight")]
    [TestCase("Jean-Jacques", "Jean", "@Jean$-Jacques", TestName = "MapRowsToHits_HighlightWithHyphen_ReturnsValidHighlight")]
    [TestCase("Jean(s)", "Jean", "@Jean$(s)", TestName = "MapRowsToHits_HighlightWithParentheses_ReturnsValidHighlight")]
    [TestCase("Pat: my friend", "Pat", "@Pat$: my friend", TestName = "MapRowsToHits_HighlightWithColon_ReturnsValidHighlight")]
    [TestCase("Jeff?", "Jeff", "@Jeff$?", TestName = "MapRowsToHits_HighlightWithQuestionMark_ReturnsValidHighlight")]
    [TestCase("Jeff", "Jef", null, TestName = "MapRowsToHits_HighlightWithSubstring_DoesNotHighlight")]
    [TestCase("Jeff0", "Jeff", null, TestName = "MapRowsToHits_HighlightWithSubstringSpecialChar_DoesNotHighlight")]
    [TestCase("Jeff", "JEfF", "@Jeff$", TestName = "MapRowsToHits_HighlightCaseInsensitive_ReturnsValidHighlight")]
    [TestCase("Jeff", "label1:JEfF", "@Jeff$", TestName = "MapRowsToHits_HighlightWithFieldName_ReturnsValidHighlight")]
    [TestCase("Stella Maria Joe and friends", "Joe AND Stella", "@Stella$ Maria @Joe$ and friends", TestName = "MapRowsToHits_HighlightWithLuceneAnd_ReturnsValidHighlight")]
    [TestCase("jakarta Apache apache lucene apache jakarta", "\"jakarta apache\" NOT \"Apache Lucene\"", "@jakarta$ @Apache$ apache lucene apache jakarta", TestName = "MapRowsToHits_HighlightWithLuceneNot_ReturnsValidHighlight")]
    [TestCase("jakarta Apache", "jakarta^4", "@jakarta$ Apache", TestName = "MapRowsToHits_HighlightWithCtrl_ReturnsValidHighlight")]
    [TestCase("Jakarta website", "(jakarta OR apache) AND website", "@Jakarta$ @website$", TestName = "MapRowsToHits_HighlightWithLuceneAndOr_ReturnsValidHighlight")]
    [TestCase("return of the pink panther pink", "title:(+return +\"pink panther\")", "@return$ of the @pink$ @panther$ pink", TestName = "MapRowsToHits_HighlightWithLuceneAndFieldName_ReturnsValidHighlight")]

    [TestCase("Bob's your uncle", "Bob", "@Bob$'s your uncle")]
    [TestCase("Joe.did.it", "Joe", "@Joe$.did.it")]
    [TestCase("Joe.did.it", "Joe.did.it", "@Joe$.@did$.@it$")]
    [TestCase("1.3 123", "1.3", "@1$.@3$ 123")]
    [TestCase("Pat:my friend", "Pat", "@Pat$:my friend")]
    public void HitCreateWithHighlightsAdvancedCases(string text, string highlightString, string expectedString)
    {
        // Arrange
        using var table = SampleData();
        table.Rows[0]["label1"] = text;
        var query = new QueryData(
            "myKQL",
            "myIndex",
            highlightText: new Dictionary<string, string> {
                    { "*", highlightString },
            })
        {
            HighlightPreTag = "@",
            HighlightPostTag = "$",
        };

        // Assert
        expected[0]["_source"]["label1"] = JToken.FromObject(text);
        if (expectedString != null)
        {
            var highlight = JToken.FromObject(new string[] { expectedString });
            expected[0]["highlight"] = new JObject
            {
                ["label1"] = highlight,
            };
        }

        // Act
        var hits = ReadHits(table, query);

        // Assert
        AssertHits(hits);
    }

    [Test]
    [TestCase("label1", "boxes")]
    [TestCase("instant", 1483362245060)]
    [TestCase("value", 234)]
    public void MapRowsToHits_WithSort_ReturnsHitsWithSort(string field, object expectedValue)
    {
        // Arrange
        using var table = SampleData();
        var query = new QueryData(
            "myKQL",
            "myIndex",
            sortFields: new List<string> { field },
            highlightText: new Dictionary<string, string> {
                      { "*", "boxes" },
            });

        // Assert
        var highlight = JToken.FromObject(new string[] { "FooboxesBar" });
        expected[0]["highlight"] = new JObject
        {
            ["label1"] = highlight,
            ["label2"] = highlight,
            ["label3"] = JToken.FromObject(
            new string[]
            {
                    "FooboxesBar of FooboxesBar",
            }),
        };
        expected[0]["sort"] = JToken.FromObject(new object[] { expectedValue });
        query.HighlightPreTag = "Foo";
        query.HighlightPostTag = "Bar";

        // Act
        var hits = ReadHits(table, query);

        // Assert
        AssertHits(hits);
    }

    [Test]
    [TestCase("label1", "boxes")]
    [TestCase("instant", "2017-01-02T13:04:05.06Z")]
    [TestCase("value", 234)]
    public void MapRowsToHits_WithDocFields_ReturnsHitsWithFields(string field, object expectedValue)
    {
        // Arrange
        using var table = SampleData();
        var query = new QueryData(
            "myKQL",
            "myIndex",
            sortFields: null,
            docValueFields: new List<string> { field },
            highlightText: null);

        // for some reason working with date objects doesn't really work so we have
        // to actually convert it to a datetime.
        var isDate = DateTime.TryParse(expectedValue.ToString(), out var date);

        var expectedObject = new Dictionary<string, List<object>>
            {
                { field, new List<object> { isDate ? date.ToUniversalTime() : expectedValue } },
            };
        expected[0]["fields"] = JToken.FromObject(expectedObject);

        // Act
        var hits = ReadHits(table, query);

        // Assert
        AssertHits(hits);
    }

    [Test]
    public void MapRowsToHits_WithSByte_ReturnsHitsWithBoolean()
    {
        using var hitsTable = HitTypeTestTable(Type.GetType("System.SByte"), 1);
        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(true));
    }

    [Test]
    public void MapRowsToHits_WithNullSByte_ReturnsHitsWithNull()
    {
        using var hitsTable = HitTypeTestTable(Type.GetType("System.SByte"), DBNull.Value);
        MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
    }

    [Test]
    public void MapRowsToHits_WithSqlDecimal_ReturnsHitsWithDouble()
    {
        var value = new SqlDecimal(1.6);
        using var hitsTable = HitTypeTestTable(value.GetType(), value);

        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(1.6));
    }

    [Test]
    public void MapRowsToHits_WithNullSqlDecimal_ReturnsHitsWithNanDouble()
    {
        var value = new SqlDecimal(0);
        using var hitsTable = HitTypeTestTable(value.GetType(), SqlDecimal.Null);
        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(double.NaN));
    }

    [Test]
    public void MapRowsToHits_WithNullGuid_ReturnsHitsWithNull()
    {
        using var hitsTable = HitTypeTestTable(Type.GetType("System.Guid"), DBNull.Value);
        MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
    }

    [Test]
    public void MapRowsToHits_WithGuid_ReturnsHitsWithString()
    {
        var value = new Guid("74be27de-1e4e-49d9-b579-fe0b331d3642");
        using var hitsTable = HitTypeTestTable(Type.GetType("System.Guid"), value);
        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue("74be27de-1e4e-49d9-b579-fe0b331d3642"));
    }

    [Test]
    public void MapRowsToHits_WithNullTimespan_ReturnsHitsWithNull()
    {
        using var hitsTable = HitTypeTestTable(Type.GetType("System.TimeSpan"), DBNull.Value);
        MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
    }

    [Test]
    public void MapRowsToHits_WithTimespan_ReturnsHitsWithISO8601String()
    {
        var value = new TimeSpan(20000087879);
        using var hitsTable = HitTypeTestTable(Type.GetType("System.TimeSpan"), value);

        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue("PT33M20.0087879S"));
    }

    [Test]
    public void MapRowsToHits_WithNullDateTime_ReturnsHitsWithNull()
    {
        using var hitsTable = HitTypeTestTable(Type.GetType("System.DateTime"), DBNull.Value);
        MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
    }

    [Test]
    public void MapRowsToHits_WithDateTime_ReturnsHitsWithDateTimeString()
    {
        var value = DateTime.Now.ToUniversalTime();
        using var hitsTable = HitTypeTestTable(Type.GetType("System.DateTime"), value);
        var expectedString = value.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF", DateTimeFormatInfo.InvariantInfo);

        MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(expectedString));
    }

    private static DataTable SampleData()
    {
        return SampleDataWith(
            instant: new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc),
            value: 234,
            label1: "boxes",
            label2: "boxes",
            label3: "boxes of boxes");
    }

    private static DataTable SampleDataWith(string id = "", DateTime? instant = null, int value = 0, string label1 = "", string label2 = "", string label3 = "")
    {
        using var table = new DataTable();
        if (!string.IsNullOrEmpty(id))
        {
            table.Columns.Add("_id", typeof(string));
        }

        if (instant.HasValue)
        {
            table.Columns.Add("instant", typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
        }

        if (value > 0)
        {
            table.Columns.Add("value", typeof(int));
        }

        if (!string.IsNullOrEmpty(label1))
        {
            table.Columns.Add("label1", typeof(string));
        }

        if (!string.IsNullOrEmpty(label2))
        {
            table.Columns.Add("label2", typeof(string));
        }

        if (!string.IsNullOrEmpty(label3))
        {
            table.Columns.Add("label3", typeof(string));
        }

        var row = table.NewRow();

        if (!string.IsNullOrEmpty(id))
        {
            row["_id"] = id;
        }

        if (instant.HasValue)
        {
            row["instant"] = instant.Value;
        }

        if (value > 0)
        {
            row["value"] = value;
        }

        if (!string.IsNullOrEmpty(label1))
        {
            row["label1"] = label1;
        }

        if (!string.IsNullOrEmpty(label2))
        {
            row["label2"] = label2;
        }

        if (!string.IsNullOrEmpty(label3))
        {
            row["label3"] = label3;
        }

        table.Rows.Add(row);
        return table;
    }

    private static DataTable HitTypeTestTable(Type dataType, object value)
    {
        var resTable = new DataTable();

        var column1 = new DataColumn
        {
            ColumnName = "column1",
            DataType = dataType,
            AllowDBNull = true,
        };
        resTable.Columns.Add(column1);

        var row1 = resTable.NewRow();
        row1["column1"] = value;

        resTable.Rows.Add(row1);

        return resTable;
    }

    private static IEnumerable<Hit> ReadHits(DataTable table, QueryData query)
    {
        var logger = new Mock<ILogger>();
        using var highlighter = new LuceneHighlighter(query, logger.Object);
        return HitsMapper.MapRowsToHits(table.Rows, query, highlighter);
    }

    private static void MapHitTypeAndAssert(DataTable table, QueryData query, object expected)
    {
        var logger = new Mock<ILogger>();
        using var highlighter = new LuceneHighlighter(query, logger.Object);
        var hits = HitsMapper.MapRowsToHits(table.Rows, query, highlighter);
        var hitl = new List<Hit>(hits);
        Assert.AreEqual(1, hitl.Count);

        foreach (var hit in hitl)
        {
            hit.Id = null;
        }

        var hitJson = JToken.FromObject(hitl).First;

        Assert.AreEqual(hitJson["_source"]["column1"], expected);
    }

    private void AssertHits(IEnumerable<Hit> hits)
    {
        var hitl = new List<Hit>(hits);
        Assert.AreEqual(1, hitl.Count);

        foreach (var hit in hitl)
        {
            hit.Id = null;
        }

        var hitJson = JToken.FromObject(hitl);

        hitJson.Should().BeEquivalentTo(expected);
    }
}
