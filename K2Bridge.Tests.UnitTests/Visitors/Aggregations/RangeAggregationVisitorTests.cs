// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations.Bucket;
    using K2Bridge.Tests.UnitTests.Visitors;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeAggregationVisitorTests
    {
        [TestCase(0, 800, 800, 2000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%800','800%2000'), ['_range_value'] = pack_array(['dayOfWeek'] >= 0 and ['dayOfWeek'] < 800,['dayOfWeek'] >= 800 and ['dayOfWeek'] < 2000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%800','key','800%2000'] | as metadata;")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('%800','800%2000'), ['_range_value'] = pack_array(['dayOfWeek'] < 800,['dayOfWeek'] >= 800 and ['dayOfWeek'] < 2000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','%800','key','800%2000'] | as metadata;")]
        [TestCase(0, 800, 800, null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%800','800%'), ['_range_value'] = pack_array(['dayOfWeek'] >= 0 and ['dayOfWeek'] < 800,['dayOfWeek'] >= 800)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%800','key','800%'] | as metadata;")]
        [TestCase(null, 800, 800, null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('%800','800%'), ['_range_value'] = pack_array(['dayOfWeek'] < 800,['dayOfWeek'] >= 800)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','%800','key','800%'] | as metadata;")]
        [TestCase(0, 10000, 2000, 3000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%10000','2000%3000'), ['_range_value'] = pack_array(['dayOfWeek'] >= 0 and ['dayOfWeek'] < 10000,['dayOfWeek'] >= 2000 and ['dayOfWeek'] < 3000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%10000','key','2000%3000'] | as metadata;")]
        [TestCase(-800, 0, -2000, -800, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('-800%0','-2000%-800'), ['_range_value'] = pack_array(['dayOfWeek'] >= -800 and ['dayOfWeek'] < 0,['dayOfWeek'] >= -2000 and ['dayOfWeek'] < -800)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','-800%0','key','-2000%-800'] | as metadata;")]
        public string RangeVisit_WithAggregation_ReturnsValidResponse(double? from1, double? to1, double? from2, double? to2)
        {
            var rangeAggregation = new RangeAggregation()
            {
                Metric = "wibble",
                Field = "dayOfWeek",
                Key = "key",
                Ranges = new List<RangeAggregationExpression>() {
                    new RangeAggregationExpression { Field = "dayOfWeek", From = from1, To = to1 },
                    new RangeAggregationExpression { Field = "dayOfWeek", From = from2, To = to2 },
                },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("dayOfWeek", "double");
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }

        [TestCase(0, 800, 800, 2000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%800','800%2000'), ['_range_value'] = pack_array(todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 800,todouble(['dayOfWeek'].['A']) >= 800 and todouble(['dayOfWeek'].['A']) < 2000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%800','key','800%2000'] | as metadata;")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('%800','800%2000'), ['_range_value'] = pack_array(todouble(['dayOfWeek'].['A']) < 800,todouble(['dayOfWeek'].['A']) >= 800 and todouble(['dayOfWeek'].['A']) < 2000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','%800','key','800%2000'] | as metadata;")]
        [TestCase(0, 800, 800, null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%800','800%'), ['_range_value'] = pack_array(todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 800,todouble(['dayOfWeek'].['A']) >= 800)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%800','key','800%'] | as metadata;")]
        [TestCase(null, 800, 800, null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('%800','800%'), ['_range_value'] = pack_array(todouble(['dayOfWeek'].['A']) < 800,todouble(['dayOfWeek'].['A']) >= 800)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','%800','key','800%'] | as metadata;")]
        [TestCase(0, 10000, 2000, 3000, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array('0%10000','2000%3000'), ['_range_value'] = pack_array(todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 10000,todouble(['dayOfWeek'].['A']) >= 2000 and todouble(['dayOfWeek'].['A']) < 3000)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','0%10000','key','2000%3000'] | as metadata;")]
        public string RangeVisit_WithAggregation_Dynamic_ReturnsValidResponse(double? from1, double? to1, double? from2, double? to2)
        {
            var rangeAggregation = new RangeAggregation()
            {
                Metric = "wibble",
                Field = "dayOfWeek.A",
                Key = "key",
                Ranges = new List<RangeAggregationExpression>() {
                    new RangeAggregationExpression { Field = "dayOfWeek.A", From = from1, To = to1 },
                    new RangeAggregationExpression { Field = "dayOfWeek.A", From = from2, To = to2 },
                },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("dayOfWeek.A", "double");
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }

        [Test]
        public void RangeVisit_WithOpenAggregation_ReturnsValidResponse()
        {
            var rangeAggregation = new RangeAggregation()
            {
                Metric = "wibble",
                Field = "dayOfWeek",
                Key = "key",
                Ranges = new List<RangeAggregationExpression>() {
                    new RangeAggregationExpression { Field = "dayOfWeek", From = null, To = null },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            VisitorTestsUtils.VisitRootDsl(visitor);
            visitor.Visit(rangeAggregation);

            Assert.AreEqual("\nlet _extdata = _data\n| extend ['key'] = pack_array('%'), ['_range_value'] = pack_array(true)\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);\ndatatable(key:string, value:string) ['key','%'] | as metadata;", rangeAggregation.KustoQL);
        }
    }
}
