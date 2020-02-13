// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeClauseVisitorTests
    {
        // Numeric RangeClause Query Tests
        [TestCase(
            ExpectedResult = "MyField >= 0 and MyField < 10",
            TestName="Visit_WithValidRangeBetweenNumbers_ReturnsValidResponse")]
        public string TestValidRangeClauseVisitNumberBetweenTwoInts()
        {
            return RangeClauseToKQL(CreateRangeClause(0, 10m));
        }

        [TestCase(
            ExpectedResult = "MyField >= 0 and MyField < 10.10",
            TestName="Visit_WithValidRangeBetweenIntAndDecimal_ReturnsValidResponse")]
        public string TestValidRangeClauseVisitNumberBetweenIntAndDecimal()
        {
            return RangeClauseToKQL(CreateRangeClause(0, 10.10m));
        }

        [TestCase(
            ExpectedResult = "MyField >= 10.10 and MyField < 20.20",
            TestName="Visit_WithValidRangeBetweenDecimals_ReturnsValidResponse")]
        public string TestValidRangeClauseVisitNumberBetweenTwoDecimalss()
        {
            return RangeClauseToKQL(CreateRangeClause(10.10m, 20.20m));
        }

        // Time RangeClause Query Tests
        [TestCase(
            ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10)",
            TestName="Visit_WithValidRangeBetweenInts_ReturnsValidResponse")]
        public string TestValidTimeRangeClauseVisitNumberBetweenTwoInts()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(0, 10m));
        }

        [TestCase(
            ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10.10)",
            TestName="Visit_WithValidTimeRangeBetweenNumbers_ReturnsValidResponse")]
        public string TestValidTimeRangeClauseVisitNumberBetweenIntAndDecimal()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(0, 10.10m));
        }

        [TestCase(
            ExpectedResult = "MyField >= fromUnixTimeMilli(10.10) and MyField <= fromUnixTimeMilli(20.20)",
            TestName="Visit_WithValidTimeRangeBetweenDecimals_ReturnsValidResponse")]
        public string TestValidTimeRangeClauseVisitNumberBetweenTwoDecimalss()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(10.10m, 20.20m));
        }

        private static RangeClause CreateRangeClause(decimal min, decimal max)
        {
            return new RangeClause()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = null,
                LTValue = max,
                Format = null,
            };
        }

        private static RangeClause CreateTimeRangeClause(decimal min, decimal max)
        {
            return new RangeClause()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = max,
                LTValue = null,
                Format = "epoch_millis",
            };
        }

        private static string RangeClauseToKQL(RangeClause rangeClause)
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(rangeClause);
            return rangeClause.KustoQL;
        }
    }
}