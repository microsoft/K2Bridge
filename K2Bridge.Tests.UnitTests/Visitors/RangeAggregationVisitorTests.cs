// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeAggregationVisitorTests
    {
        [TestCase(0, 800, 800, 2000, ExpectedResult = "union (_data | where ['dayOfWeek'] >= 0 and ['dayOfWeek'] < 800 | summarize wibble | extend ['key'] = '0-800'), (_data | where ['dayOfWeek'] >= 800 and ['dayOfWeek'] < 2000 | summarize wibble | extend ['key'] = '800-2000') | project-reorder ['key'], * asc")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "union (_data | where ['dayOfWeek'] < 800 | summarize wibble | extend ['key'] = '-800'), (_data | where ['dayOfWeek'] >= 800 and ['dayOfWeek'] < 2000 | summarize wibble | extend ['key'] = '800-2000') | project-reorder ['key'], * asc")]
        [TestCase(0, 800, 800, null, ExpectedResult = "union (_data | where ['dayOfWeek'] >= 0 and ['dayOfWeek'] < 800 | summarize wibble | extend ['key'] = '0-800'), (_data | where ['dayOfWeek'] >= 800 | summarize wibble | extend ['key'] = '800-') | project-reorder ['key'], * asc")]
        [TestCase(null, 800, 800, null, ExpectedResult = "union (_data | where ['dayOfWeek'] < 800 | summarize wibble | extend ['key'] = '-800'), (_data | where ['dayOfWeek'] >= 800 | summarize wibble | extend ['key'] = '800-') | project-reorder ['key'], * asc")]
        [TestCase(0, 10000, 2000, 3000, ExpectedResult = "union (_data | where ['dayOfWeek'] >= 0 and ['dayOfWeek'] < 10000 | summarize wibble | extend ['key'] = '0-10000'), (_data | where ['dayOfWeek'] >= 2000 and ['dayOfWeek'] < 3000 | summarize wibble | extend ['key'] = '2000-3000') | project-reorder ['key'], * asc")]
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

        [TestCase(0, 800, 800, 2000, ExpectedResult = "union (_data | where todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 800 | summarize wibble | extend ['key'] = '0-800'), (_data | where todouble(['dayOfWeek'].['A']) >= 800 and todouble(['dayOfWeek'].['A']) < 2000 | summarize wibble | extend ['key'] = '800-2000') | project-reorder ['key'], * asc")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "union (_data | where todouble(['dayOfWeek'].['A']) < 800 | summarize wibble | extend ['key'] = '-800'), (_data | where todouble(['dayOfWeek'].['A']) >= 800 and todouble(['dayOfWeek'].['A']) < 2000 | summarize wibble | extend ['key'] = '800-2000') | project-reorder ['key'], * asc")]
        [TestCase(0, 800, 800, null, ExpectedResult = "union (_data | where todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 800 | summarize wibble | extend ['key'] = '0-800'), (_data | where todouble(['dayOfWeek'].['A']) >= 800 | summarize wibble | extend ['key'] = '800-') | project-reorder ['key'], * asc")]
        [TestCase(null, 800, 800, null, ExpectedResult = "union (_data | where todouble(['dayOfWeek'].['A']) < 800 | summarize wibble | extend ['key'] = '-800'), (_data | where todouble(['dayOfWeek'].['A']) >= 800 | summarize wibble | extend ['key'] = '800-') | project-reorder ['key'], * asc")]
        [TestCase(0, 10000, 2000, 3000, ExpectedResult = "union (_data | where todouble(['dayOfWeek'].['A']) >= 0 and todouble(['dayOfWeek'].['A']) < 10000 | summarize wibble | extend ['key'] = '0-10000'), (_data | where todouble(['dayOfWeek'].['A']) >= 2000 and todouble(['dayOfWeek'].['A']) < 3000 | summarize wibble | extend ['key'] = '2000-3000') | project-reorder ['key'], * asc")]
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

            Assert.AreEqual("union (_data | summarize wibble | extend ['key'] = '-') | project-reorder ['key'], * asc", rangeAggregation.KustoQL);
        }
    }
}
