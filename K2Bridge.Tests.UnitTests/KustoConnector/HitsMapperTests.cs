// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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
    }
}