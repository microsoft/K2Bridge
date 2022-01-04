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
    public class DateRangeVisitorTests
    {
        [TestCase("2018-02-01", "2018-02-02", "2018-02-02", "2018-02-3", ExpectedResult = "union (_data | where ['dayOfWeek'] >= make_datetime('2018-02-01') and ['dayOfWeek'] < make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-01'), '_', make_datetime('2018-02-02'))), (_data | where ['dayOfWeek'] >= make_datetime('2018-02-02') and ['dayOfWeek'] < make_datetime('2018-02-3') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-02'), '_', make_datetime('2018-02-3'))) | order by ['key'] asc | project-reorder ['key'], * asc")]
        [TestCase(null, "2018-02-02", "2018-02-02", "2018-02-3", ExpectedResult = "union (_data | where ['dayOfWeek'] < make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat('', '_', make_datetime('2018-02-02'))), (_data | where ['dayOfWeek'] >= make_datetime('2018-02-02') and ['dayOfWeek'] < make_datetime('2018-02-3') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-02'), '_', make_datetime('2018-02-3'))) | order by ['key'] asc | project-reorder ['key'], * asc")]
        [TestCase("2018-02-01", "2018-02-02", "2018-02-02", null, ExpectedResult = "union (_data | where ['dayOfWeek'] >= make_datetime('2018-02-01') and ['dayOfWeek'] < make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-01'), '_', make_datetime('2018-02-02'))), (_data | where ['dayOfWeek'] >= make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-02'), '_', '')) | order by ['key'] asc | project-reorder ['key'], * asc")]
        [TestCase(null, "2018-02-02", "2018-02-02", null, ExpectedResult = "union (_data | where ['dayOfWeek'] < make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat('', '_', make_datetime('2018-02-02'))), (_data | where ['dayOfWeek'] >= make_datetime('2018-02-02') | summarize wibble | extend ['key'] = strcat(make_datetime('2018-02-02'), '_', '')) | order by ['key'] asc | project-reorder ['key'], * asc")]
        public string DateRangeVisit_WithAggregation_ReturnsValidResponse(string from1, string to1, string from2, string to2)
        {
            var rangeAggregation = new DateRangeAggregation()
            {
                Metric = "wibble",
                Field = "dayOfWeek",
                Key = "key",
                Ranges = new List<DateRangeAggregationExpression>() {
                    new DateRangeAggregationExpression { Field = "dayOfWeek", From = from1, To = to1 },
                    new DateRangeAggregationExpression { Field = "dayOfWeek", From = from2, To = to2 },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            VisitorTestsUtils.VisitRootDsl(visitor);
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }
    }
}
