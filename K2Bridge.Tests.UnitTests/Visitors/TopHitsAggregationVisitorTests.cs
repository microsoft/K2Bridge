// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TopHitsAggregationVisitorTests
    {
        [TestCase(ExpectedResult = "\nlet _tophits2 = _extdata\n| join kind=inner _summarizablemetrics on ['3']\n| partition by ['3'] (top 1 by ['sortfieldA'] desc\n| project ['2']=pack('source_field','metricfieldA','source_value',['metricfieldA'],'sort_value',['sortfieldA'])\n| summarize ['2%tophits']=make_list(['2']));")]
        public string TopHitsAggregationVisit_ReturnsValidResponse()
        {
            var topHitsAggregation = new TopHitsAggregation()
            {
                DocValueFields = new List<DocValueField>() { new DocValueField() { Field = "metricfieldA" } },
                Key = "2",
                PartitionKey="3",
                Field = "metricfieldA",
                Size = 1,
                Sort = new List<SortClause>() { new SortClause() { FieldName = "sortfieldA", Order = "desc" } },
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockTimestampSchemaRetriever());
            VisitorTestsUtils.VisitRootDsl(visitor);
            visitor.Visit(topHitsAggregation);

            return topHitsAggregation.KustoQL;
        }

        [TestCase(ExpectedResult = "\nlet _tophits2 = _extdata\n| join kind=inner _summarizablemetrics on ['3']\n| partition by ['3'] (top 1 by todouble(['sortfieldA'].['B']) desc\n| project ['2']=pack('source_field','metricfieldA.B','source_value',['metricfieldA'].['B'],'sort_value',['sortfieldA'].['B'])\n| summarize ['2%tophits']=make_list(['2']));")]
        public string TopHitsAggregationVisit_WithDynamic_ReturnsValidResponse()
        {
            var topHitsAggregation = new TopHitsAggregation()
            {
                DocValueFields = new List<DocValueField>() { new DocValueField() { Field = "metricfieldA.B" } },
                Key = "2",
                PartitionKey="3",
                Field = "metricfieldA.B",
                Size = 1,
                Sort = new List<SortClause>() { new SortClause() { FieldName = "sortfieldA.B", Order = "desc" } },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("sortfieldA.B", "long");
            visitor.Visit(topHitsAggregation);

            return topHitsAggregation.KustoQL;
        }
    }
}
