namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;

    [TestFixture]
    public class TestRangeClauseVisitorTests
    {
        //Numeric Range Query Tests
        [TestCase(ExpectedResult = "MyField >= 0 and MyField < 10")]
        public string TestValidRangeClauseVisitNumberBetweenTwoInts()
        {
            return RangeClauseToKQL(CreateRangeClause(0, 10m));
        }

        [TestCase(ExpectedResult = "MyField >= 0 and MyField < 10.10")]
        public string TestValidRangeClauseVisitNumberBetweenIntAndDecimal()
        {
            return RangeClauseToKQL(CreateRangeClause(0, 10.10m));
        }

        [TestCase(ExpectedResult = "MyField >= 10.10 and MyField < 20.20")]
        public string TestValidRangeClauseVisitNumberBetweenTwoDecimalss()
        {
            return RangeClauseToKQL(CreateRangeClause(10.10m, 20.20m));
        }

        //Time Range Query Tests
        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10)")]
        public string TestValidTimeRangeClauseVisitNumberBetweenTwoInts()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(0, 10m));
        }

        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10.10)")]
        public string TestValidTimeRangeClauseVisitNumberBetweenIntAndDecimal()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(0, 10.10m));
        }

        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(10.10) and MyField <= fromUnixTimeMilli(20.20)")]
        public string TestValidTimeRangeClauseVisitNumberBetweenTwoDecimalss()
        {
            return RangeClauseToKQL(CreateTimeRangeClause(10.10m, 20.20m));
        }

        private static K2Bridge.Models.Request.Queries.Range CreateRangeClause(decimal min, decimal max)
        {
            return new K2Bridge.Models.Request.Queries.Range()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = null,
                LTValue = max,
                Format = null
            };
        }

        private static K2Bridge.Models.Request.Queries.Range CreateTimeRangeClause(decimal min, decimal max)
        {
            return new K2Bridge.Models.Request.Queries.Range()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = max,
                LTValue = null,
                Format = "epoch_millis"
            };
        }

        private static string RangeClauseToKQL(K2Bridge.Models.Request.Queries.Range range)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(range);
            return range.KQL;
        }
    } 
}