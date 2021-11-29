// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TermsVisitorTests
    {
        [TestCase("_count", ExpectedResult = "wibble by _alias = wobble\n| order by count_ desc\n| limit 10", TestName = "TermsVisit_WithAggregationSortCount_ReturnsValidResponse")]
        [TestCase("_key", ExpectedResult = "wibble by _alias = wobble\n| order by _alias desc\n| limit 10", TestName = "TermsVisit_WithAggregationKeyCount_ReturnsValidResponse")]
        public string TermsVisit_WithAggregation_ReturnsValidResponse(string sortFieldName)
        {
            var termsAggregation = new TermsAggregation()
            {
                Metric = "wibble",
                FieldName = "wobble",
                FieldAlias = "_alias",
                Size = 10,
                SortFieldName = sortFieldName,
                SortOrder = "desc",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(termsAggregation);

            return termsAggregation.KustoQL;
        }
    }
}
