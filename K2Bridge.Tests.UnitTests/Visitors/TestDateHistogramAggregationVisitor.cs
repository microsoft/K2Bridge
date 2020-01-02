// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace VisitorsTests
{
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TestDateHistogramAggregationVisitor
    {
        [TestCase(ExpectedResult = "wibble by wobble = wobble\n| order by wobble asc")]
        public string DateHistogramAggregationConstructsQuery()
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KQL;
        }

        [TestCase("w", ExpectedResult = "wibble by wobble = startofweek(wobble)\n| order by wobble asc")]
        [TestCase("y", ExpectedResult = "wibble by wobble = startofyear(wobble)\n| order by wobble asc")]
        [TestCase("M", ExpectedResult = "wibble by wobble = startofmonth(wobble)\n| order by wobble asc")]
        [TestCase("z", ExpectedResult = "wibble by wobble = bin(wobble, z)\n| order by wobble asc")]
        public string DateHistogramAggregationWithStartOfWeekIntervalConstructsQuery(string interval)
        {
            var histogramAggregation = new DateHistogramAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
                Interval = interval,
            };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(histogramAggregation);

            return histogramAggregation.KQL;
        }
    }
}
