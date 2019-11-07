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
        public string AggregationHasNoPrimary_DoesNotSetQuery()
        {
            var aggregateClause = new Aggregation();

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }

        [TestCase(ExpectedResult = "avg(wibble)")]
        public string AggregationWithPrimary_GeneratesQuery()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new Avg() { FieldName = "wibble" }
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }

        [TestCase(ExpectedResult = "avg(bibble), avg(wibble)")]
        public string AggregationWithSubAggregates_GeneratesQuery()
        {
            var aggregateClause = new Aggregation()
            {
                PrimaryAggregation = new Avg() { FieldName = "wibble" },
                SubAggregations = new Dictionary<string, Aggregation>
                {
                    { "wobble", new Aggregation() { PrimaryAggregation = new Avg { FieldName = "bibble" } } }
                }
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(aggregateClause);

            return aggregateClause.KQL;
        }
    }
}
