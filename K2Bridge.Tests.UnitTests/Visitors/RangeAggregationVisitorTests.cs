// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeAggregationVisitorTests
    {
        [TestCase(0, 800, 800, 2000, ExpectedResult = "wibble by ['_alias'] = case(dayOfWeek >= 0 and dayOfWeek < 800, '0-800', dayOfWeek >= 800 and dayOfWeek < 2000, '800-2000', 'default_bucket')")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "wibble by ['_alias'] = case(dayOfWeek < 800, '-800', dayOfWeek >= 800 and dayOfWeek < 2000, '800-2000', 'default_bucket')")]
        [TestCase(0, 800, 800, null, ExpectedResult = "wibble by ['_alias'] = case(dayOfWeek >= 0 and dayOfWeek < 800, '0-800', dayOfWeek >= 800, '800-', 'default_bucket')")]
        [TestCase(null, 800, 800, null, ExpectedResult = "wibble by ['_alias'] = case(dayOfWeek < 800, '-800', dayOfWeek >= 800, '800-', 'default_bucket')")]
        public string RangeVisit_WithAggregation_ReturnsValidResponse(double? from1, double? to1, double? from2, double? to2)
        {
            var rangeAggregation = new RangeAggregation()
            {
                Metric = "wibble",
                FieldName = "dayOfWeek",
                FieldAlias = "_alias",
                Ranges = new List<RangeAggregationExpression>() {
                    new RangeAggregationExpression { From = from1, To = to1 },
                    new RangeAggregationExpression { From = from2, To = to2 },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            VisitorTestsUtils.VisitRootDsl(visitor);
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }
    }
}
