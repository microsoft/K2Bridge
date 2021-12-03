// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class AggregationVisitorTests
    {
        [TestCase(ExpectedResult = null)]
        public string AggregationVisit_WithEmptyAgg_ReturnsNoPrimary()
        {
            var aggregateClause = new AggregationContainer();

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=avg(fieldA)")]
        public string AggregationVisit_WithAvgAgg_ReturnsAvgPrimary()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new AverageAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['B']=avg(fieldB), ['A']=avg(fieldA)")]
        public string AggregationVisit_WithSubAvgAgg_ReturnsAvgAggregates()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new AverageAggregation() { Field = "fieldA", Key = "A" },
                SubAggregations = new AggregationDictionary
                {
                    { "sub", new AggregationContainer() { PrimaryAggregation = new AverageAggregation { Field = "fieldB", Key = "B" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(fieldA)")]
        public string AggregationVisit_WithCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['B']=dcount(fieldB), ['A']=dcount(fieldA)")]
        public string AggregationVisit_WithSubCardinalityAggs_ReturnsDCounts()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA", Key = "A" },
                SubAggregations = new AggregationDictionary
                {
                    { "sub", new AggregationContainer() { PrimaryAggregation = new CardinalityAggregation { Field = "fieldB", Key = "B" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=min(fieldA)")]
        public string AggregationVisit_WithMinAgg_ReturnsMinPrimary()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['B']=min(fieldB), ['A']=min(fieldA)")]
        public string AggregationVisit_WithSubMinAgg_ReturnsMinAggregates()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA", Key = "A" },
                SubAggregations = new AggregationDictionary
                {
                    { "sub", new AggregationContainer() { PrimaryAggregation = new MinAggregation { Field = "fieldB", Key = "B" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=max(fieldA)")]
        public string AggregationVisit_WithMaxAgg_ReturnsMaxPrimary()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['B']=max(fieldB), ['A']=max(fieldA)")]
        public string AggregationVisit_WithSubMaxAgg_ReturnsMaxAggregates()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA", Key = "A" },
                SubAggregations = new AggregationDictionary
                {
                    { "sub", new AggregationContainer() { PrimaryAggregation = new MaxAggregation { Field = "fieldB", Key = "B" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=sum(fieldA)")]
        public string AggregationVisit_WithSumAgg_ReturnsSumPrimary()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new SumAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['B']=sum(fieldB), ['A']=sum(fieldA)")]
        public string AggregationVisit_WithSubSumAgg_ReturnsSumAggregates()
        {
            var aggregateClause = new AggregationContainer()
            {
                PrimaryAggregation = new SumAggregation() { Field = "fieldA", Key = "A" },
                SubAggregations = new AggregationDictionary
                {
                    { "sub", new AggregationContainer() { PrimaryAggregation = new SumAggregation { Field = "fieldB", Key = "B" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['_A%50.0']=percentile(fieldA, 50)")]
        public string AggregationVisit_WithPercentileAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new PercentileAggregation() { FieldName = "fieldA", FieldAlias = "_A", Percents = new double[] { 50 } },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['_B%50.0']=percentile(fieldB, 50), ['_A%50.0']=percentile(fieldA, 50)")]
        public string AggregationVisit_WithSubPercentileAgg_ReturnsPercentileAggregates()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new PercentileAggregation() { FieldName = "fieldA", FieldAlias = "_A", Percents = new double[] { 50 } },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "sub", new Aggregation() { PrimaryAggregation = new PercentileAggregation { FieldName = "fieldB", FieldAlias = "_B", Percents = new double[] { 50 } } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }
    }
}
