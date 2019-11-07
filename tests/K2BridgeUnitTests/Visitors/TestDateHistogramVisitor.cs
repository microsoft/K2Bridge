using System;
using K2Bridge.Models.Request.Aggregations;
using K2Bridge.Visitors;
using NUnit.Framework;

namespace VisitorsTests
{
    [TestFixture]
    public class TestDateHistogramVisitor
    {
        [TestCase(ExpectedResult = "wibble by wobble = wobble | order by todatetime(wobble) asc")]
        public string DateHistogramConstructsQuery()
        {
            var histogram = new DateHistogram()
            {
                Metric = "wibble",
                FieldName = "wobble"
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(histogram);

            return histogram.KQL;
        }

        [TestCase("w", ExpectedResult = "wibble by wobble = startofweek(wobble) | order by todatetime(wobble) asc")]
        [TestCase("y", ExpectedResult = "wibble by wobble = startofyear(wobble) | order by todatetime(wobble) asc")]
        [TestCase("M", ExpectedResult = "wibble by wobble = startofmonth(wobble) | order by todatetime(wobble) asc")]
        [TestCase("z", ExpectedResult = "wibble by wobble = bin(todatetime(wobble), z) | order by todatetime(wobble) asc")]
        public string DateHistogramWithStartOfWeekIntervalConstructsQuery(string interval)
        {
            var histogram = new DateHistogram()
            {
                Metric = "wibble",
                FieldName = "wobble",
                Interval = interval
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(histogram);

            return histogram.KQL;
        }
    }
}
