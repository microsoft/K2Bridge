// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class RangeAggregationVisitorTests
    {
        [TestCase(0, 800, 800, 2000, ExpectedResult = "wibble by ['_alias'] = case(wobble >= 0 and wobble < 800, '0-800', wobble >= 800 and wobble < 2000, '800-2000', 'default_bucket')")]
        [TestCase(null, 800, 800, 2000, ExpectedResult = "wibble by ['_alias'] = case(wobble < 800, '-800', wobble >= 800 and wobble < 2000, '800-2000', 'default_bucket')")]
        [TestCase(0, 800, 800, null, ExpectedResult = "wibble by ['_alias'] = case(wobble >= 0 and wobble < 800, '0-800', wobble >= 800, '800-', 'default_bucket')")]
        [TestCase(null, 800, 800, null, ExpectedResult = "wibble by ['_alias'] = case(wobble < 800, '-800', wobble >= 800, '800-', 'default_bucket')")]
        public string RangeVisit_WithAggregation_ReturnsValidResponse(double? from1, double? to1, double? from2, double? to2)
        {
            var rangeAggregation = new RangeAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
                FieldAlias = "_alias",
                Ranges = new List<RangeAggregationExpression>() {
                    new RangeAggregationExpression { From = from1, To = to1 },
                    new RangeAggregationExpression { From = from2, To = to2 },
                },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }
    }
}
