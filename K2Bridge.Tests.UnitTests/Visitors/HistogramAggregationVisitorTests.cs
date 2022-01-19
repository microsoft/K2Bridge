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
        [TestCase(ExpectedResult = "\nlet _extdata = _data\n| extend ['key%False'] = bin(['field'], 20);\nlet _summarizablemetrics = _extdata\n| summarize count() by ['key%False']\n| order by ['key%False'] asc;")]
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

        [TestCase(ExpectedResult = "\nlet _extdata = _data\n| extend ['key%False'] = bin(['field'], 20);\nlet _summarizablemetrics = _extdata\n| summarize count() by ['key%False']\n| order by ['key%False'] asc;")]
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

        [TestCase(ExpectedResult = "\nlet _extdata = _data\n| extend ['key%False'] = bin(['field'], 20)\n| where ['field'] >= bin(50, 20) and ['field'] < bin(150, 20)+20;\n" +
                                   "let _summarizablemetrics = _extdata\n| summarize count() by ['key%False']\n| order by ['key%False'] asc;")]
        public string HistogramVisitWithHardBounds_WithSimpleAggregation_ReturnsValidResponse()
        {
            var histogramAggregation = new HistogramAggregation()
            {
                Field = "field",
                Interval = 20,
                HardBounds = new Bounds
                {
                    Min = 50,
                    Max = 150,
                },
                Key = "key",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KustoQL;
        }
    }
}