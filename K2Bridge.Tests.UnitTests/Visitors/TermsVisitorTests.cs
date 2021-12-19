// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.


namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TermsVisitorTests
    {
        [TestCase("_count", ExpectedResult = "_data | summarize metric by ['key'] = field\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "_data | summarize metric by ['key'] = field\n| order by ['key'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "_data | summarize metric by ['key'] = field\n| order by ['1'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation() {
                Field = "field",
                Key = "key",
                Size = 10,
                Order = new TermsOrder() {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "_data | summarize metric by ['key'] = tostring(field.A)\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "_data | summarize metric by ['key'] = tostring(field.A)\n| order by ['key'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "_data | summarize metric by ['key'] = tostring(field.A)\n| order by ['1'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicString_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation() {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder() {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "string");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by ['key'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by ['1'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicLong_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation() {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder() {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "long");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by ['key'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "_data | summarize metric by ['key'] = todouble(field.A)\n| order by ['1'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicDouble_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation() {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder() {
                    SortField = sortFieldName,
                    SortOrder = "desc",
                },
                Metric = "metric",
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("field.A", "double");
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }

        [TestCase("_count", ExpectedResult = "_data | summarize metric by ['key'] = todatetime(field.A)\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "_data | summarize metric by ['key'] = todatetime(field.A)\n| order by ['key'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        [TestCase("1", ExpectedResult = "_data | summarize metric by ['key'] = todatetime(field.A)\n| order by ['1'] desc\n| limit 10", TestName = "TermsVisit_WithAggregationCustomCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_DynamicDate_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation() {
                Field = "field.A",
                Key = "key",
                Size = 10,
                Order = new TermsOrder() {
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
