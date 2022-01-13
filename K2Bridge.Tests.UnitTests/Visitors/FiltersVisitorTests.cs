// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class FiltersVisitorTests
    {
        [TestCase("foo:bar", "bar:baz", ExpectedResult = "_data | extend ['_tmp'] = pack_array('foo:bar','bar:baz'), ['_filter_value'] = pack_array((foo has \"bar\"),(bar has \"baz\")) | mv-expand ['_tmp'] to typeof(string), ['_filter_value'] | where ['_filter_value'] == true | summarize wibble by ['_tmp'] | order by ['_tmp'] asc | project-rename ['key%Zm9vOmJhcg--%YmFyOmJheg--']=['_tmp']")]
        [TestCase("foo:(bar OR baz)", "bar:(baz OR foo)", ExpectedResult = "_data | extend ['_tmp'] = pack_array('foo:(bar OR baz)','bar:(baz OR foo)'), ['_filter_value'] = pack_array(((foo has \"bar\") or (foo has \"baz\")),((bar has \"baz\") or (bar has \"foo\"))) | mv-expand ['_tmp'] to typeof(string), ['_filter_value'] | where ['_filter_value'] == true | summarize wibble by ['_tmp'] | order by ['_tmp'] asc | project-rename ['key%Zm9vOihiYXIgT1IgYmF6KQ--%YmFyOihiYXogT1IgZm9vKQ--']=['_tmp']")]
        [TestCase("foo", "*", ExpectedResult = "_data | extend ['_tmp'] = pack_array('foo','*'), ['_filter_value'] = pack_array((* has \"foo\"),(true)) | mv-expand ['_tmp'] to typeof(string), ['_filter_value'] | where ['_filter_value'] == true | summarize wibble by ['_tmp'] | order by ['_tmp'] asc | project-rename ['key%Zm9v%Kg--']=['_tmp']")]
        [TestCase("foo", "bar:*", ExpectedResult = "_data | extend ['_tmp'] = pack_array('foo','bar:*'), ['_filter_value'] = pack_array((* has \"foo\"),(bar matches regex \"(.)*\")) | mv-expand ['_tmp'] to typeof(string), ['_filter_value'] | where ['_filter_value'] == true | summarize wibble by ['_tmp'] | order by ['_tmp'] asc | project-rename ['key%Zm9v%YmFyOio-']=['_tmp']")]
        public string FiltersVisit_WithAggregation_ReturnsValidResponse(string q1, string q2)
        {
            var rangeAggregation = new FiltersAggregation()
            {
                Metric = "wibble",
                Key = "key",
                Filters = new Dictionary<string, FiltersBoolQuery>()
                {
                    [q1] = new FiltersBoolQuery
                    {
                        BoolQuery = new BoolQuery()
                        {
                            Must = new List<QueryStringClause>()
                            {
                                new QueryStringClause()
                                {
                                    Phrase = q1,
                                    Wildcard = true,
                                    Default = "*",
                                },
                            },
                        },
                    },
                    [q2] = new FiltersBoolQuery
                    {
                        BoolQuery = new BoolQuery()
                        {
                            Must = new List<QueryStringClause>()
                            {
                                new QueryStringClause()
                                {
                                    Phrase = q2,
                                    Wildcard = true,
                                    Default = "*",
                                },
                            },
                        },
                    },
                },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("dayOfWeek", "double");
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }
    }
}
