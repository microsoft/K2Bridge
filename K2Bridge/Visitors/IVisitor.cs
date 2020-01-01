// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;

    /// <summary>
    /// IVisitor defines all the different visit methods overloads to handle all supported query types
    /// </summary>
    internal interface IVisitor
    {
        void Visit(ExistsClause existsClause);

        void Visit(MatchPhraseClause matchPhraseClause);

        void Visit(Query query);

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

        void Visit(QueryStringClause queryStringClause);
    }
}
