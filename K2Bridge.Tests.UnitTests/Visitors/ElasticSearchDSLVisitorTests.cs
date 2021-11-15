// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System;
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticSearchDSLVisitorTests
    {
        [TestCase(ExpectedResult =
            "let _data = database(\"\").myindex | where (dayOfWeek == 1);\n(_data | limit 0 | as hits)")]
        public string Visit_WithNumericFieldType_GeneratesQueryWithEqual()
        {
            var queryClause = CreateQueryStringClause("dayOfWeek:1", false);
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

        [TestCase(ExpectedResult =
        "let _data = database(\"\").myindex | where (dayOfWeek >2);\n(_data | limit 0 | as hits)")]
        public string Visit_WithGreaterThanExpression_ExpectedResults()
        {
            var queryClause = CreateQueryStringClause("dayOfWeek:>2", false);
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

        [TestCase(ExpectedResult =
            "let _data = database(\"\").myindex | where (dayOfWeek has \"1\");\n(_data | limit 0 | as hits)")]
        public string Visit_WithStringFieldType_GeneratesQueryWithHas()
        {
            var queryClause = CreateQueryStringClause("dayOfWeek:1", false);
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

            Assert.AreEqual(dsl.KustoQL, "let _data = database(\"defaultDBName\").someindex " + string.Empty + ";\n(_data | limit 0 | as hits)");
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

            Assert.True(dsl.KustoQL.Contains("let _data = database(\"defaultDBName\").someindex", StringComparison.OrdinalIgnoreCase));
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
                Aggregations = new Dictionary<string, Aggregation>(),
            };

            CreateVisitorAndVisit(dsl);

            Assert.False(dsl.KustoQL.Contains("| as aggs);", StringComparison.OrdinalIgnoreCase));
            Assert.False(dsl.KustoQL.Contains("summarize);", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Visit_WhenHasAggregations_KustoQLShouldContainAggs()
        {
            var agg = JsonConvert.DeserializeObject<Aggregation>(@"
                {
                    ""date_histogram"": {
                        ""field"": ""timestamp"",
                        ""fixed_interval"": ""3h"",
                        ""time_zone"": ""Asia/Jerusalem"",
                        ""min_doc_count"": 1
                    }
                }");

            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
                Aggregations = new Dictionary<string, Aggregation>() { { "2", agg } },
            };

            CreateVisitorAndVisit(dsl);

            Assert.True(dsl.KustoQL.Contains("_data | summarize count() by timestamp = bin(timestamp, 3h)\n", StringComparison.OrdinalIgnoreCase));
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

            Assert.True(dsl.KustoQL.Contains("\n(_data | order by timestamp asc | limit 17", StringComparison.OrdinalIgnoreCase));
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

            Assert.False(dsl.KustoQL.Contains("\n(_data | order by timestamp asc", StringComparison.OrdinalIgnoreCase));
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

        private void CreateVisitorAndVisit(ElasticSearchDSL elasticSearchDSL, string dbName = "")
        {
            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockSchemaRetriever(), dbName);
            visitor.Visit(elasticSearchDSL);
        }
    }
}