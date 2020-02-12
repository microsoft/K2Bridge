// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;

    /// <summary>
    /// IVisitor defines all the different visit methods overloads to handle all supported query types.
    /// </summary>
    internal interface IVisitor
    {
        void Visit(ExistsClause existsClause);

        void Visit(MatchPhraseClause matchPhraseClause);

        void Visit(Query query);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="elasticSearchDSL">An Elasticsearch DSL query.</param>
        void Visit(ElasticSearchDSL elasticSearchDSL);

        void Visit(RangeClause rangeClause);

        void Visit(BoolQuery boolQuery);

        void Visit(SortClause sortClause);

        void Visit(Aggregation aggregation);

        // Bucket Aggregations
        void Visit(DateHistogramAggregation dateHistogramAggregation);

        // Metric Aggregations
        void Visit(AvgAggregation avgAggregation);

        void Visit(CardinalityAggregation cardinalityAggregation);

        /// <summary>
        /// Accepts a query string clause, parses the phrase to a lucene query, and builds a Kusto query based on the lucene query.
        /// </summary>
        /// <param name="queryStringClause">The query string clause.</param>
        void Visit(QueryStringClause queryStringClause);
    }
}
