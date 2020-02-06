// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace Tests
{
    using System;
    using K2Bridge;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
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
        public void IllegalClauseExceptionHasDefaultCtor()
        {
            try
            {
                throw new IllegalClauseException();
            }
            catch (Exception exc)
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
        public void IllegalClauseExceptionHasCustomCtor()
        {
            var customMsg = "custom message";
            try
            {
                throw new IllegalClauseException(customMsg);
            }
            catch (Exception exc)
            {
                Assert.AreEqual(exc.Message, customMsg);
            }
        }

        /// <summary>
        /// Has a ctor with inner exception.
        /// </summary>
        [TestCase]
        public void IllegalClauseExceptionHasCustomWithInnerExcCtor()
        {
            var customMsg = "custom message";
            var innerMsg = "inner exc message";
            try
            {
                throw new IllegalClauseException(customMsg, new ArgumentException(innerMsg));
            }
            catch (Exception exc)
            {
                Assert.AreEqual(exc.Message, customMsg);
                Assert.AreEqual(exc.InnerException.Message, innerMsg);
            }
        }

        // The following tests verify that given an invalid clause, they
        // throw an <see cref="IllegalClauseException"/> or ArgumentException
        [TestCase]
        public void TestInvalidExistsClause()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new ExistsClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((ExistsClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidMatchPhraseClause()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new MatchPhraseClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((MatchPhraseClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidAvgAggMetric()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new AvgAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((AvgAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidAggMetric()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            // This is a valid scenario
            visitor.Visit(new Aggregation());

            Assert.That(
                () => visitor.Visit((Aggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidCardinalityAggMetric()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new CardinalityAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((CardinalityAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidRangeClause()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new RangeClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((RangeClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidSortClause()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new SortClause()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((SortClause)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidDateHistogramAgg()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new DateHistogramAggregation()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((DateHistogramAggregation)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidBoolQuery()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            // This is a valid scenario
            visitor.Visit(new BoolQuery());

            Assert.That(
                () => visitor.Visit((BoolQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase]
        public void TestInvalidQuery()
        {
            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());

            Assert.That(
                () => visitor.Visit(new Query()),
                Throws.TypeOf<IllegalClauseException>());

            Assert.That(
                () => visitor.Visit((BoolQuery)null),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}