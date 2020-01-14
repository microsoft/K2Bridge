// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace VisitorsTests
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TestAggregationVisitor
    {
        [TestCase(ExpectedResult = null)]
        public string TestMetricAggregationHasNoPrimary()
        {
            var aggregateClause = new Aggregation();

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }

        [TestCase(ExpectedResult = "avg(fieldA)")]
        public string TestMetricAggregationWithPrimary_Avg()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new AvgAggregation() { FieldName = "fieldA" },
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }

        [TestCase(ExpectedResult = "avg(fieldB), avg(fieldA)")]
        public string TestMetricAggregationWithSubAggregates_Avg()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new AvgAggregation() { FieldName = "fieldA" },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "sub", new Aggregation() { PrimaryAggregation = new AvgAggregation { FieldName = "fieldB" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }


        [TestCase(ExpectedResult = "dcount(fieldA)")]
        public string TestMetricAggregation_Cardinality()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new CardinalityAggregation() { FieldName = "fieldA" },
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }

        [TestCase(ExpectedResult = "dcount(fieldB), dcount(fieldA)")]
        public string TestMetricAggregationWithSubAggregates_DCount()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new CardinalityAggregation() { FieldName = "fieldA" },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "sub", new Aggregation() { PrimaryAggregation = new CardinalityAggregation { FieldName = "fieldB" } } },
                },
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }
    }
}
