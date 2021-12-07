// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class DateHistogramAggregationVisitorTests
    {
        [TestCase(ExpectedResult = "metric by ['key'] = field\n| order by ['key'] asc")]
        public string DateHistogramVisit_WithSimpleAggregation_ReturnsValidResponse()
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Field = "field",
                Key = "key",
                Metric = "metric",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }

        [TestCase("w", ExpectedResult = "metric by ['key'] = startofweek(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("week", ExpectedResult = "metric by ['key'] = startofweek(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("y", ExpectedResult = "metric by ['key'] = startofyear(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("year", ExpectedResult = "metric by ['key'] = startofyear(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("M", ExpectedResult = "metric by ['key'] = startofmonth(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("month", ExpectedResult = "metric by ['key'] = startofmonth(field)\n| order by ['key'] asc", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("z", ExpectedResult = "metric by ['key'] = bin(field, z)\n| order by ['key'] asc")]
        public string DateHistogramVisit_WithAggregation_ReturnsValidResponse(string interval)
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Field = "field",
                FixedInterval = interval,
                Key = "key",
                Metric = "metric",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }
    }
}
