// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeVisitorTests
    {
        [TestCase(
            ExpectedResult = "['myField'] >= 3 and ['myField'] < 5",
            TestName = "Visit_WithBasicInput_ReturnsValidResponse")]
        public string TestBasicRangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", "3", null, null, "5", "other");

            return VisitRangeClause(rangeClause, "myField", "integer");
        }

        [TestCase(
            ExpectedResult =
            "['myField'] >= unixtime_milliseconds_todatetime(1212121121) and ['myField'] <= unixtime_milliseconds_todatetime(2121212121)",
            TestName = "Visit_WithEpochInput_ReturnsValidResponse")]
        public string TestEpochBasicRangeVisitor()
        {
            var rangeClause = CreateRangeClause("myField", "1212121121", null, "2121212121", null, "epoch_millis");

            return VisitRangeClause(rangeClause);
        }

        [Test]
        public void Visit_WithMissingFieldNameInput_ThrowsException()
        {
            var rangeClause = CreateRangeClause(null, "3", null, null, "5", "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [Test]
        public void Visit_WithMissingGTEInput_ThrowsException()
        {
            var rangeClause = CreateRangeClause("myField", null, null, null, "5", "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause, type: "integer"));
        }

        [Test]
        public void Visit_WithMissingLTEInput_ThrowsException()
        {
            var rangeClause = CreateRangeClause("myField", "5", null, null, null, "other");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause, type: "integer"));
        }

        [Test]
        public void Visit_WithEpochMissingGTEInput_ThrowsException()
        {
            var rangeClause = CreateRangeClause("myField", null, null, null, "5", "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        [Test]
        public void Visit_WithEpochMissingLTEInput_ThrowsException()
        {
            var rangeClause = CreateRangeClause("myField", "5", null, null, null, "epoch_millis");

            Assert.Throws(typeof(IllegalClauseException), () => VisitRangeClause(rangeClause));
        }

        private static string VisitRangeClause(RangeClause clause, string fieldName = "myField", string type = "string")
        {
            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor(fieldName, type);
            visitor.Visit(clause);
            return clause.KustoQL;
        }

        private static RangeClause CreateRangeClause(
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable SA1114 // Parameter list should follow declaration
            string fieldName, string gte, string gt, string lte, string lt, string format)
#pragma warning restore SA1114 // Parameter list should follow declaration
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            return new RangeClause
            {
                FieldName = fieldName,
                GTEValue = gte,
                GTValue = gt,
                LTValue = lt,
                LTEValue = lte,
                Format = format,
            };
        }
    }
}
