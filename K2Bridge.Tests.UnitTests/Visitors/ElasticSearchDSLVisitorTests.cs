// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticSearchDSLVisitorTests
    {
        [TestCase("dayOfWeek", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'] == 1);\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        [TestCase("dayOfWeek.a.b", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'].['a'].['b'] == 1);\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        public string Visit_WithNumericFieldType_GeneratesQueryWithEqual(string fieldName)
        {
            var queryClause = CreateQueryStringClause(fieldName + ":1", false);
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery
                    {
                        Must = new List<IQuery> { queryClause },
                    },
                },
                IndexName = "myindex",
            };

            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            visitor.Visit(dsl);
            return dsl.KustoQL;
        }

        [TestCase("dayOfWeek", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'] >2);\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        [TestCase("dayOfWeek.a.b", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'].['a'].['b'] >2);\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        public string Visit_WithGreaterThanExpression_ExpectedResults(string fieldName)
        {
            var queryClause = CreateQueryStringClause(fieldName + ":>2", false);
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery
                    {
                        Must = new List<IQuery> { queryClause },
                    },
                },
                IndexName = "myindex",
            };

            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            visitor.Visit(dsl);
            return dsl.KustoQL;
        }

        [TestCase("dayOfWeek", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'] has \"1\");\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        [TestCase("dayOfWeek.a.b", ExpectedResult = "let _data = database(\"\").['myindex'] | where (['dayOfWeek'].['a'].['b'] has \"1\");\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)")]
        public string Visit_WithStringFieldType_GeneratesQueryWithHas(string fieldName)
        {
            var queryClause = CreateQueryStringClause(fieldName + ":1", false);
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery
                    {
                        Must = new List<IQuery> { queryClause },
                    },
                },
                IndexName = "myindex",
            };

            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(dsl);
            return dsl.KustoQL;
        }

        [Test]
        public void Visit_CreatedWithEmptyQuery_ConvertsToEmptyString()
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
            };

            CreateVisitorAndVisit(dsl, "defaultDBName");

            Assert.AreEqual(dsl.KustoQL, "let _data = database(\"defaultDBName\").['someindex'] " + string.Empty + ";\n(_data | count | as hitsTotal);\n(_data | limit 0 | as hits)");
        }

        [Test]
        public void Visit_CreatedWithDefaultDBName_UsesDefaultDBInQuery()
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
            };

            CreateVisitorAndVisit(dsl, "defaultDBName");

            Assert.True(dsl.KustoQL.Contains("let _data = database(\"defaultDBName\").['someindex']", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasNullAggregations_KustoQLContainNoAggs()
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
            };

            CreateVisitorAndVisit(dsl);

            Assert.False(dsl.KustoQL.Contains("| as aggs);", StringComparison.OrdinalIgnoreCase));
            Assert.False(dsl.KustoQL.Contains("summarize);", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasEmptyAggregations_KustoQLContainNoAggs()
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Aggregations = new AggregationDictionary(),
            };

            CreateVisitorAndVisit(dsl);

            Assert.False(dsl.KustoQL.Contains("| as aggs);", StringComparison.OrdinalIgnoreCase));
            Assert.False(dsl.KustoQL.Contains("summarize);", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasAggregations_KustoQLShouldContainAggs()
        {
            var dateHistogramAggregation = @"
                {""aggs"": {
                    ""2"": {
                        ""date_histogram"": {
                            ""field"": ""timestamp"",
                            ""fixed_interval"": ""1m"",
                            ""time_zone"": ""Asia/Jerusalem"",
                            ""min_doc_count"": 1
                        }
                    }
                }}";

            var aggs = JsonConvert.DeserializeObject<AggregationContainer>(dateHistogramAggregation);

            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Aggregations = aggs.SubAggregations,
            };

            CreateVisitorAndVisit(dsl);

            Assert.True(dsl.KustoQL.Contains("\nlet _extdata = _data\n| extend ['2'] = bin(['timestamp'], 1m);\nlet _summarizablemetrics = _extdata\n| summarize count() by ['2']\n", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasSortClauseAndSize_KustoQLContainsSortClauseAndLimit()
        {
            var sortClause = JsonConvert.DeserializeObject<SortClause>(@"
                                {
                                    ""timestamp"": {
                                        ""order"": ""asc"",
                                        ""unmapped_type"": ""boolean""
                                    }
                                }");

            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Size = 17,
                Sort = new List<SortClause> { sortClause },
            };

            CreateVisitorAndVisit(dsl);

            Assert.True(dsl.KustoQL.Contains("\n(_data | order by ['timestamp'] asc | limit 17", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasSortClauseButZeroSize_KustoQLOrderingIgnored()
        {
            var sortClause = JsonConvert.DeserializeObject<SortClause>(@"
                                {
                                    ""timestamp"": {
                                        ""order"": ""asc"",
                                        ""unmapped_type"": ""boolean""
                                    }
                                }");

            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Size = 0,
                Sort = new List<SortClause> { sortClause },
            };

            CreateVisitorAndVisit(dsl);

            Assert.False(dsl.KustoQL.Contains("\n(_data | order by ['timestamp'] asc", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WithRandomSizeValue_KustoQLContainsLimitAndHits()
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Size = 17,
                Sort = new List<SortClause>(),
            };

            CreateVisitorAndVisit(dsl);

            Assert.True(dsl.KustoQL.Contains("limit 17 | as hits", StringComparison.OrdinalIgnoreCase));
        }

        private static QueryStringClause CreateQueryStringClause(string phrase, bool wildcard)
        {
            return new QueryStringClause
            {
                Phrase = phrase,
                Wildcard = wildcard,
                Default = "*",
            };
        }

        private static void CreateVisitorAndVisit(ElasticSearchDSL elasticSearchDSL, string dbName = "")
        {
            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockSchemaRetriever(), dbName);
            visitor.Visit(elasticSearchDSL);
        }
    }
}
