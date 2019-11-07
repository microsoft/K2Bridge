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
    public class TestRangeQueryVisitorTests
    {
        //Numeric Range Query Tests
        [TestCase(ExpectedResult = "MyField >= 0 and MyField < 10")]
        public string TestValidRangeQueryVisitNumberBetweenTwoInts()
        {
            return RangeQueryToKQL(CreateRangeQuery(0, 10m));
        }

        [TestCase(ExpectedResult = "MyField >= 0 and MyField < 10.10")]
        public string TestValidRangeQueryVisitNumberBetweenIntAndDecimal()
        {
            return RangeQueryToKQL(CreateRangeQuery(0, 10.10m));
        }

        [TestCase(ExpectedResult = "MyField >= 10.10 and MyField < 20.20")]
        public string TestValidRangeQueryVisitNumberBetweenTwoDecimalss()
        {
            return RangeQueryToKQL(CreateRangeQuery(10.10m, 20.20m));
        }

        //Time Range Query Tests
        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10)")]
        public string TestValidTimeRangeQueryVisitNumberBetweenTwoInts()
        {
            return RangeQueryToKQL(CreateTimeRangeQuery(0, 10m));
        }

        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(0) and MyField <= fromUnixTimeMilli(10.10)")]
        public string TestValidTimeRangeQueryVisitNumberBetweenIntAndDecimal()
        {
            return RangeQueryToKQL(CreateTimeRangeQuery(0, 10.10m));
        }

        [TestCase(ExpectedResult = "MyField >= fromUnixTimeMilli(10.10) and MyField <= fromUnixTimeMilli(20.20)")]
        public string TestValidTimeRangeQueryVisitNumberBetweenTwoDecimalss()
        {
            return RangeQueryToKQL(CreateTimeRangeQuery(10.10m, 20.20m));
        }

        private static RangeQuery CreateRangeQuery(decimal min, decimal max)
        {
            return new RangeQuery()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = null,
                LTValue = max,
                Format = null
            };
        }

        private static RangeQuery CreateTimeRangeQuery(decimal min, decimal max)
        {
            return new RangeQuery()
            {
                FieldName = "MyField",
                GTEValue = min,
                GTValue = null,
                LTEValue = max,
                LTValue = null,
                Format = "epoch_millis"
            };
        }

        private static string RangeQueryToKQL(RangeQuery rangeQuery)
        {
            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(rangeQuery);
            return rangeQuery.KQL;
        }
    } 
}