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
    }
}
