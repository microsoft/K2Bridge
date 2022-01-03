// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class HistogramAggregationVisitorTests
    {
        [TestCase(ExpectedResult = "_data | summarize count() by ['key'] = bin(['field'], 20)\n| order by ['key'] asc")]
        public string HistogramVisit_WithSimpleAggregation_ReturnsValidResponse()
        {
            var histogramAggregation = new HistogramAggregation()
            {
                Field = "field",
                Interval = 20,
                MinimumDocumentCount = 0,
                Key = "key",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }

        [TestCase(ExpectedResult = "_data | summarize count() by ['key'] = bin(['field'], 20)\n| order by ['key'] asc")]
        public string HistogramVisitWithMinDocCount_WithSimpleAggregation_ReturnsValidResponse()
        {
            var histogramAggregation = new HistogramAggregation()
            {
                Field = "field",
                Interval = 20,
                MinimumDocumentCount = 1,
                Key = "key",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }
    }
}