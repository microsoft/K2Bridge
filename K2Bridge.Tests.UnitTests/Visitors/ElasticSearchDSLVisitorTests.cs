// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class ElasticSearchDSLVisitorTests
    {
        [TestCase(ExpectedResult =
            "let fromUnixTimeMilli = (t:long) {datetime(1970 - 01 - 01) + t * 1millisec};\nlet _data = database(\"\").myindex | where (dayOfWeek == 1);\n(_data | limit 0 | as hits)")]
        public string Visit_WithTypeNumeric_GeneratesQueryWithEqual()
        {
            var queryClause = CreateQueryStringClause("dayOfWeek:1", false);
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
            };
            dsl.Query.Bool.Must = new List<IQuery> { queryClause };
            dsl.IndexName = "myindex";

            var visitor =
                new ElasticSearchDSLVisitor(
                    SchemaRetrieverMock.CreateMockNumericSchemaRetriever());
            visitor.Visit(dsl);
            return dsl.KustoQL;
        }

        [TestCase(ExpectedResult =
            "let fromUnixTimeMilli = (t:long) {datetime(1970 - 01 - 01) + t * 1millisec};\nlet _data = database(\"\").myindex | where (dayOfWeek has \"1\");\n(_data | limit 0 | as hits)")]
        public string Visit_WithTypeStrubg_GeneratesQueryWithhHas()
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

        private static QueryStringClause CreateQueryStringClause(string phrase, bool wildcard)
        {
            return new QueryStringClause
            {
                Phrase = phrase,
                Wildcard = wildcard,
                Default = "*",
            };
        }
    }
}