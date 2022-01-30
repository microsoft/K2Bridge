// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors.Aggregations;

using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Tests.UnitTests.Visitors;
using K2Bridge.Visitors;
using NUnit.Framework;

[TestFixture]
public class DateHistogramAggregationVisitorTests
{
    [TestCase(ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = ['field'];\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
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

    [TestCase("w", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofweek(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
    [TestCase("week", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofweek(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
    [TestCase("y", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofyear(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
    [TestCase("year", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofyear(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
    [TestCase("M", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofmonth(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
    [TestCase("month", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofmonth(['field']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
    [TestCase("z", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = bin(['field'], z);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
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

    [TestCase("w", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofweek(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
    [TestCase("week", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofweek(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfWeekInterval_ReturnsValidResponse")]
    [TestCase("y", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofyear(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
    [TestCase("year", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofyear(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfYearInterval_ReturnsValidResponse")]
    [TestCase("M", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofmonth(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
    [TestCase("month", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = startofmonth(todatetime(['field'].['A']));\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);", TestName = "DateHistogramVisit_WithStartOfMonthInterval_ReturnsValidResponse")]
    [TestCase("z", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = bin(todatetime(['field'].['A']), z);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
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
