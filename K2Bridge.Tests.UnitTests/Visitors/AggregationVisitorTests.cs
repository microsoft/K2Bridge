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
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new AverageAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=avg(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicLongAvgAgg_ReturnsAvgPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new AverageAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=avg(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicDoubleAvgAgg_ReturnsAvgPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new AverageAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(fieldA)")]
        public string AggregationVisit_WithCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(tostring(fieldA.B))")]
        public string AggregationVisit_WithDynamicStringCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "string");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicLongCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicDoubleCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=dcount(todatetime(fieldA.B))")]
        public string AggregationVisit_WithDynamicDatetimeCardinalityAgg_ReturnsDCount()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new CardinalityAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "date");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=min(fieldA)")]
        public string AggregationVisit_WithMinAgg_ReturnsMinPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=min(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicLongMinAgg_ReturnsMinPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=min(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicDoubleMinAgg_ReturnsMinPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=min(todatetime(fieldA.B))")]
        public string AggregationVisit_WithDynamicDatetimeMinAgg_ReturnsMinPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MinAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "date");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=max(fieldA)")]
        public string AggregationVisit_WithMaxAgg_ReturnsMaxPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=max(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicLongMaxAgg_ReturnsMaxPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=max(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicDoubleMaxAgg_ReturnsMaxPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=max(todatetime(fieldA.B))")]
        public string AggregationVisit_WithDynamicDatetimeMaxAgg_ReturnsMaxPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new MaxAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "date");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=sum(fieldA)")]
        public string AggregationVisit_WithSumAgg_ReturnsSumPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new SumAggregation() { Field = "fieldA", Key = "A" },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=sum(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicLongSumAgg_ReturnsSumPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new SumAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A']=sum(todouble(fieldA.B))")]
        public string AggregationVisit_WithDynamicDoubleSumAgg_ReturnsSumPrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new SumAggregation() { Field = "fieldA.B", Key = "A" },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%50.0%False']=percentiles_array(fieldA, 50)")]
        public string AggregationVisit_WithPercentileAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA", Key = "A", Percents = new double[] { 50 }, Keyed = false },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%50.0%False']=percentiles_array(todouble(fieldA.B), 50)")]
        public string AggregationVisit_WithDynamicLongPercentileAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA.B", Key = "A", Percents = new double[] { 50 }, Keyed = false },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%50.0%False']=percentiles_array(todouble(fieldA.B), 50)")]
        public string AggregationVisit_WithDynamicDoublePercentileAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA.B", Key = "A", Percents = new double[] { 50 }, Keyed = false },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%25.0%50.0%99.0%False']=percentiles_array(fieldA, 25,50,99)")]
        public string AggregationVisit_WithPercentilesAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA", Key = "A", Percents = new double[] { 25, 50, 99 }, Keyed = false },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%25.0%50.0%99.0%False']=percentiles_array(todouble(fieldA.B), 25,50,99)")]
        public string AggregationVisit_WithDynamicLongPercentilesAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA.B", Key = "A", Percents = new double[] { 25, 50, 99 }, Keyed = false },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "long");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }

        [TestCase(ExpectedResult = "['A%percentile%25.0%50.0%99.0%False']=percentiles_array(todouble(fieldA.B), 25,50,99)")]
        public string AggregationVisit_WithDynamicDoublePercentilesAgg_ReturnsPercentilePrimary()
        {
            var aggregateClause = new AggregationContainer() {
                PrimaryAggregation = new PercentileAggregation() { Field = "fieldA.B", Key = "A", Percents = new double[] { 25, 50, 99 }, Keyed = false },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("fieldA.B", "double");
            visitor.Visit(aggregateClause);

            return aggregateClause.KustoQL;
        }
    }
}
