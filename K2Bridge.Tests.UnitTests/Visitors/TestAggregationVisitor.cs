using System;
using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Aggregations;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
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
                PrimaryAggregation = new AvgAggregation() { FieldName = "fieldA" }
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
                    { "sub", new Aggregation() { PrimaryAggregation = new AvgAggregation { FieldName = "fieldB" } } }
                }
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
                PrimaryAggregation = new CardinalityAggregation() { FieldName = "fieldA" }
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
                    { "sub", new Aggregation() { PrimaryAggregation = new CardinalityAggregation { FieldName = "fieldB" } } }
                }
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }
    }
}
