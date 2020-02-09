// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using FluentAssertions;
    using FluentAssertions.Json;
    using K2Bridge.KustoConnector;
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
        public void HitCreate()
        {
            // Arrange
            using var table = SampleData();
            var query = new QueryData(
                "myKQL",
                "myIndex",
                highlightText: new Dictionary<string, string> { });

            // Act
            MapHitAndAssert(table, query);
        }

        [Test]
        public void HitCreateWithHighlights()
        {
            // Arrange
            using var table = SampleData();
            var query = new QueryData(
                "myKQL",
                "myIndex",
                highlightText: new Dictionary<string, string> {
                    { "*", "boxes" },
                });
            query.HighlightPreTag = "Foo";
            query.HighlightPostTag = "Bar";

            // Assert
            var highlight = JToken.FromObject(new string[] { "FooboxesBar" });
            expected[0]["highlight"] = new JObject();
            expected[0]["highlight"]["label1"] = highlight;
            expected[0]["highlight"]["label2"] = highlight;
            expected[0]["highlight"]["label3"] = JToken.FromObject(new string[] { "FooboxesBar of FooboxesBar" });

            // Act
            MapHitAndAssert(table, query);
        }

        /// TODO: fix bug that lucene parse throws excpetion when getting text of type *term.
        [TestCase("return of the pink panther pink", "title:*return*", "@return$ of the pink panther pink")]
        [Ignore("Bug #1658")]
        public void HitCreateWithHighlightsLuceneSpecialCases(string text, string highlightString, string expectedString)
        {
            HitCreateWithHighlightsAdvancedCases(text, highlightString, expectedString);
        }

        [Test]
        [TestCase("Robert 'Bob' Kennedy", "Bob", "Robert '@Bob$' Kennedy")]
        [TestCase("Jean-Jacques", "Jean", "@Jean$-Jacques")]
        [TestCase("Jean(s)", "Jean", "@Jean$(s)")]
        [TestCase("Pat: my friend", "Pat", "@Pat$: my friend")]
        [TestCase("Jeff?", "Jeff", "@Jeff$?")]
        [TestCase("Jeff", "Jef", null)]
        [TestCase("Jeff0", "Jeff", null)]
        [TestCase("Jeff", "JEfF", "@Jeff$")]
        [TestCase("Jeff", "label1:JEfF", "@Jeff$")]
        [TestCase("Stella Maria Joe and friends", "Joe AND Stella", "@Stella$ Maria @Joe$ and friends")]
        [TestCase("jakarta Apache apache lucene apache jakarta", "\"jakarta apache\" NOT \"Apache Lucene\"", "@jakarta$ @Apache$ apache lucene apache jakarta")]
        [TestCase("jakarta Apache", "jakarta^4", "@jakarta$ Apache")]
        [TestCase("Jakarta website", "(jakarta OR apache) AND website", "@Jakarta$ @website$")]
        [TestCase("return of the pink panther pink", "title:(+return +\"pink panther\")", "@return$ of the @pink$ @panther$ pink")]

        // TODO Elasticsearch treats ":", "'", "." specially, this is not implemented
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1477
        // [TestCase("Bob's your uncle", "Bob", null)]
        // [TestCase("Joe.did.it", "Joe", null)]
        // [TestCase("Joe.did.it", "Joe.did.it", "@Joe.did.it$")]
        // [TestCase("1.3 123", "1.3", "@1.3$ 123")]
        // [TestCase("Pat:my friend", "Pat", null)]
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
                });
            query.HighlightPreTag = "@";
            query.HighlightPostTag = "$";

            // Assert
            expected[0]["_source"]["label1"] = JToken.FromObject(text);
            if (expectedString != null)
            {
                var highlight = JToken.FromObject(new string[] { expectedString });
                expected[0]["highlight"] = new JObject();
                expected[0]["highlight"]["label1"] = highlight;
            }

            // Act
            MapHitAndAssert(table, query);
        }

        [Test]
        public void HitCreateWithSortAndHighlights()
        {
            // Arrange
            using var table = SampleData();
            var query = new QueryData(
                "myKQL",
                "myIndex",
                sortFields: new List<string> { "label1" },
                highlightText: new Dictionary<string, string> {
                      { "*", "boxes" },
                });
            query.HighlightPreTag = "Foo";
            query.HighlightPostTag = "Bar";

            // Assert
            expected[0]["sort"] = JToken.FromObject(new object[] { "boxes" });

            var highlight = JToken.FromObject(new string[] { "FooboxesBar" });
            expected[0]["highlight"] = new JObject();
            expected[0]["highlight"]["label1"] = highlight;
            expected[0]["highlight"]["label2"] = highlight;
            expected[0]["highlight"]["label3"] = JToken.FromObject(new string[] { "FooboxesBar of FooboxesBar" });

            // Act
            MapHitAndAssert(table, query);
        }

        [Test]
        [TestCase("label1", "boxes")]
        [TestCase("instant", 1483362245060)]
        [TestCase("value", 234)]
        public void HitCreateWithSort(string field, object expectedValue)
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
            expected[0]["highlight"] = new JObject();
            expected[0]["highlight"]["label1"] = highlight;
            expected[0]["highlight"]["label2"] = highlight;
            expected[0]["highlight"]["label3"] = JToken.FromObject(
                new string[]
                {
                    "FooboxesBar of FooboxesBar",
                });
            expected[0]["sort"] = JToken.FromObject(new object[] { expectedValue });
            query.HighlightPreTag = "Foo";
            query.HighlightPostTag = "Bar";

            // Act
            MapHitAndAssert(table, query);
        }

        [Test]
        public void Create_SourceWithSByte_ConvertsToBoolean()
        {
            using var hitsTable = HitTypeTestTable(Type.GetType("System.SByte"), 1);
            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(true));
        }

        [Test]
        public void Create_SourceWithSqlDecimal_ConvertsToDouble()
        {
            var value = new SqlDecimal(1.6);
            using var hitsTable = HitTypeTestTable(value.GetType(), value);

            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(1.6));
        }

        [Test]
        public void Create_SourceWithNullSqlDecimal_ConvertsToNaNDouble()
        {
            var value = new SqlDecimal(0);
            using var hitsTable = HitTypeTestTable(value.GetType(), SqlDecimal.Null);
            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(double.NaN));
        }

        [Test]
        public void Create_SourceWithNullGuid_ConvertsToNull()
        {
            using var hitsTable = HitTypeTestTable(Type.GetType("System.Guid"), DBNull.Value);
            MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
        }

        [Test]
        public void Create_SourceWithGuid_ConvertsToString()
        {
            var value = new Guid("74be27de-1e4e-49d9-b579-fe0b331d3642");
            using var hitsTable = HitTypeTestTable(Type.GetType("System.Guid"), value);
            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue("74be27de-1e4e-49d9-b579-fe0b331d3642"));
        }

        [Test]
        public void Create_SourceWithNullTimespan_ConvertsToNull()
        {
            using var hitsTable = HitTypeTestTable(Type.GetType("System.TimeSpan"), DBNull.Value);
            MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
        }

        [Test]
        public void Create_SourceWithTimespan_ConvertsToISO8601String()
        {
            var value = new TimeSpan(20000087879);
            using var hitsTable = HitTypeTestTable(Type.GetType("System.TimeSpan"), value);

            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue("PT33M20.0087879S"));
        }

        [Test]
        public void Create_SourceWithNullDateTime_ConvertsToNull()
        {
            using DataTable hitsTable = HitTypeTestTable(Type.GetType("System.DateTime"), DBNull.Value);
            MapHitTypeAndAssert(hitsTable, defaultQuery, JValue.CreateNull());
        }

        [Test]
        public void Create_SourceWithDateTime_ConvertsToDateTimeString()
        {
            var value = DateTime.Now.ToUniversalTime();
            using var hitsTable = HitTypeTestTable(Type.GetType("System.DateTime"), value);
            var expectedString = value.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF", DateTimeFormatInfo.InvariantInfo);

            MapHitTypeAndAssert(hitsTable, defaultQuery, new JValue(expectedString));
        }

        private static DataTable SampleData()
        {
            var table = new DataTable();
            table.Columns.Add("instant", typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
            table.Columns.Add("value", typeof(int));
            table.Columns.Add("label1", typeof(string));
            table.Columns.Add("label2", typeof(string));
            table.Columns.Add("label3", typeof(string));
            var row = table.NewRow();
            row["instant"] = new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc);
            row["value"] = 234;
            row["label1"] = "boxes";
            row["label2"] = "boxes";
            row["label3"] = "boxes of boxes";
            table.Rows.Add(row);
            return table;
        }

        private static DataTable HitTypeTestTable(Type dataType, object value)
        {
            DataTable resTable = new DataTable();

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

        private void MapHitAndAssert(DataTable table, QueryData query)
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

            var hitJson = JToken.FromObject(hitl);

            hitJson.Should().BeEquivalentTo(expected);
        }

        private void MapHitTypeAndAssert(DataTable table, QueryData query, object expected)
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
    }
}