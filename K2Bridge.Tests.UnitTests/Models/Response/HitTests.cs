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
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class HitTests
    {
        private DataRow row;

        private JToken expected;

        [SetUp]
        public void SetUp()
        {
            using var table = new DataTable();
            table.Columns.Add("instant", typeof(DateTime)).DateTimeMode = DataSetDateTime.Utc;
            table.Columns.Add("value", typeof(int));
            table.Columns.Add("label1", typeof(string));
            table.Columns.Add("label2", typeof(string));
            table.Columns.Add("label3", typeof(string));
            row = table.NewRow();
            row["instant"] = new DateTime(2017, 1, 2, 13, 4, 5, 60, DateTimeKind.Utc);
            row["value"] = 234;
            row["label1"] = "boxes";
            row["label2"] = "boxes";
            row["label3"] = "boxes of boxes";

            var expectedStr = @"{
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
            }";
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
            var query = new QueryData(
                "myKQL",
                "myIndex",
                sortFields: new List<string> { },
                highlightText: new Dictionary<string, string> { });

            // Act
            var hit = Hit.Create(row, query);

            // Assert
            var hitJson = JToken.FromObject(hit);
            hitJson.Should().BeEquivalentTo(expected);
        }

        // TODO fix highlighting behavior and enable this test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1621
        [Test]
        [Ignore("Bug #1621")]
        public void HitCreateWithHighlights()
        {
            // Arrange
            var query = new QueryData(
                "myKQL",
                "myIndex",
                sortFields: new List<string> { },
                highlightText: new Dictionary<string, string> {
                    { "*", "boxes" },
                });
            query.HighlightPreTag = "Foo";
            query.HighlightPostTag = "Bar";

            // Act
            var hit = Hit.Create(row, query);

            // Assert
            var highlight = JToken.FromObject(new string[] { "FooboxesBar" });
            expected["highlight"] = new JObject();
            expected["highlight"]["label1"] = highlight;
            expected["highlight"]["label2"] = highlight;
            var hitJson = JToken.FromObject(hit);
            hitJson.Should().BeEquivalentTo(expected);
        }
    }
}
