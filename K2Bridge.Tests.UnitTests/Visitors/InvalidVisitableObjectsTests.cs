// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors
{
    using System;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Aggregations.Bucket;
    using K2Bridge.Models.Request.Aggregations.Metric;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class InvalidVisitableObjectsTests
    {
        // verify the IllegalClauseException has at least the 3 common CTors (best practice).

        /// <summary>
        /// Has a default ctor.
        /// </summary>
        [TestCase]
        public void IllegalClauseExceptionDefaultCtor_Invoke_ReturnsIllegalClauseException()
        {
            try
            {
                throw new IllegalClauseException();
            }
            catch (IllegalClauseException exc)
            {
                Assert.AreEqual(
                    exc.Message,
                    "Clause is missing mandatory properties or has invalid values");
            }
        }

        /// <summary>
        /// Has a custom message ctor.
        /// </summary>
        [TestCase]
        public void IllegalClauseExceptionCustomCtor_Invoke_ReturnsIllegalClauseException()
        {
            var customMsg = "custom message";
            try
            {
                throw new IllegalClauseException(customMsg);
            }
            catch (IllegalClauseException exc)
            {
                Assert.AreEqual(exc.Message, customMsg);
            }
        }

        /// <summary>
        /// Has a ctor with inner exception.
        /// </summary>
        [TestCase]
        public void IllegalClauseExceptionInnerExcCtor_Invoke_ReturnsIllegalClauseException()
        {
            var customMsg = "custom message";
            var innerMsg = "inner exc message";
            try
            {
                throw new IllegalClauseException(customMsg, new ArgumentException(innerMsg));
            }
            catch (IllegalClauseException exc)
            {
                Assert.AreEqual(exc.Message, customMsg);
                Assert.AreEqual(exc.InnerException.Message, innerMsg);
            }
        }

        // The following tests verify that given an invalid clause, they
        // throw an <see cref="IllegalClauseException"/> or ArgumentException
        [TestCase]
        public void ExistsClauseVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new ExistsClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((ExistsClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void MatchPhraseClauseVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new MatchPhraseClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((MatchPhraseClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void AvgAggMetricVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new AverageAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((AverageAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void AggregationMetricVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            // This is a valid scenario
            visitor.Visit(new AggregationContainer());

            Assert.That(
                () => visitor.Visit((AggregationContainer)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void CardinalityAggMetricVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new CardinalityAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((CardinalityAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void RangeClauseVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new RangeClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((RangeClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void SortClauseVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new SortClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((SortClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void DateHistogramAggVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new DateHistogramAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((DateHistogramAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void HistogramAggVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new HistogramAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((HistogramAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void BoolQueryVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            // This is a valid scenario
            visitor.Visit(new BoolQuery());

            Assert.That(
                () => visitor.Visit((BoolQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void InvalidQueryVisit_WithInvalidClause_ThrowsIllegalClauseException()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new Query()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((BoolQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}
