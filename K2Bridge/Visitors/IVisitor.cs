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
        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="existsClause">The ExistsClause object to visit.</param>
        void Visit(ExistsClause existsClause);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="matchPhraseClause">The MatchPhraseClause object to visit.</param>
        void Visit(MatchPhraseClause matchPhraseClause);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="query">The Query object to visit.</param>
        void Visit(Query query);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="elasticSearchDSL">An Elasticsearch DSL query.</param>
        void Visit(ElasticSearchDSL elasticSearchDSL);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="rangeClause">The rangeClause to visit.</param>
        void Visit(RangeClause rangeClause);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query for data.
        /// </summary>
        /// <param name="boolQuery">The boolQuery to visit.</param>
        void Visit(BoolQuery boolQuery);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="sortClause">The sortClause to visit.</param>
        void Visit(SortClause sortClause);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="aggregation">The aggregation to visit.</param>
        void Visit(Aggregation aggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="dateHistogramAggregation">The Bucket Aggregations to visit.</param>
        void Visit(DateHistogramAggregation dateHistogramAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="avgAggregation">The Metric Aggregations to visit.</param>
        void Visit(AvgAggregation avgAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="cardinalityAggregation">The Cardinality Aggregations to visit.</param>
        void Visit(CardinalityAggregation cardinalityAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="maxAggregation">The Cardinality Aggregations to visit.</param>
        void Visit(MaxAggregation maxAggregation);

        /// <summary>
        /// Accepts a query string clause, parses the phrase to a lucene query, and builds a Kusto query based on the lucene query.
        /// </summary>
        /// <param name="queryStringClause">The query string clause.</param>
        void Visit(QueryStringClause queryStringClause);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto single doc query.
        /// </summary>
        /// <param name="documentIds">The DocumentIds object to visit.</param>
        void Visit(DocumentIds documentIds);
    }
}
