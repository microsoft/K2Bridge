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
        [TestCase(ExpectedResult = "wibble by wobble = wobble\n| order by wobble asc")]
        public string DateHistogramVisit_WithSimpleAggregation_ReturnsValidResponse()
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }

        [TestCase("w", ExpectedResult = "wibble by wobble = startofweek(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("week", ExpectedResult = "wibble by wobble = startofweek(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("y", ExpectedResult = "wibble by wobble = startofyear(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("year", ExpectedResult = "wibble by wobble = startofyear(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("M", ExpectedResult = "wibble by wobble = startofmonth(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("month", ExpectedResult = "wibble by wobble = startofmonth(wobble)\n| order by wobble asc", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("z", ExpectedResult = "wibble by wobble = bin(wobble, z)\n| order by wobble asc")]
        public string DateHistogramVisit_WithAggregation_ReturnsValidResponse(string interval)
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
                Interval = interval,
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }
    }
}
