// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors.Aggregations
{
    using K2Bridge.Models.Request.Aggregations.Bucket;
    using K2Bridge.Tests.UnitTests.Visitors;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TermsAggregationVisitorTests
    {
        [TestCase(ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = ['field'];\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| limit 10;\n(_summarizablemetrics\n| as aggs);")]
        public string TermsVisit_WithNullOrder_ReturnsValidResponse()
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field",
                Key = "key",
                Size = 10,
                Order = null,
                Metric = "metric",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = ['field'];\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['count_'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['count_'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = ['field'];\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['key'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = ['field'];\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['1'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['1'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field",
                Key = "key",
                Size = 10,
                Order = new TermsOrder()
                {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = tostring(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['count_'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['count_'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = tostring(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['key'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = tostring(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['1'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['1'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicString_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder()
                {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "string");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['count_'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['count_'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['key'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['1'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['1'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicLong_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder()
                {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "long");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['count_'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['count_'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['key'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todouble(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['1'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['1'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicDouble_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder()
                {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "double");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todatetime(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['count_'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['count_'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todatetime(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['key'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['key'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = todatetime(['field'].['A']);\nlet _summarizablemetrics = _extdata\n| summarize metric by ['key']\n| order by ['1'] desc\n| limit 10;\n(_summarizablemetrics\n| order by ['1'] desc\n| as aggs);", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicDate_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder()
                {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "date");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }
    }
}
