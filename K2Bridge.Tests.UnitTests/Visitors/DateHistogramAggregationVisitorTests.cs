﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class DateHistogramAggregationVisitorTests
    {
        [TestCase(ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = field | order by ['key'] asc;")]
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

        [TestCase("w", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofweek(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("week", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofweek(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("y", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofyear(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("year", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofyear(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("M", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofmonth(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("month", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = startofmonth(['field']) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("z", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = ['field'];metric by ['key'] = bin(['field'], z) | order by ['key'] asc;")]
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

        [TestCase("w", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofweek(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("week", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofweek(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
        [TestCase("y", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofyear(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("year", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofyear(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
        [TestCase("M", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofmonth(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("month", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = startofmonth(todatetime(['field'].['A'])) | order by ['key'] asc;", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
        [TestCase("z", ExpectedResult = "\nlet _extdata = _data | extend ['key'] = todatetime(['field'].['A']);metric by ['key'] = bin(todatetime(['field'].['A']), z) | order by ['key'] asc;")]
        public string DateHistogramVisit_WithAggregation_WithDynamicField_ReturnsValidResponse(string interval)
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Field = "field.A",
                FixedInterval = interval,
                Key = "key",
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "date");
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }
    }
}
