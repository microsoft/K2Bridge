// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Aggregations.Bucket;
    using K2Bridge.Models.Request.Aggregations.Metric;
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
        /// <param name="aggregationDictionary">The aggregation to visit.</param>
        void Visit(AggregationDictionary aggregationDictionary);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="aggregationContainer">The aggregation to visit.</param>
        void Visit(AggregationContainer aggregationContainer);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="defaultAggregation">The Bucket Aggregations to visit.</param>
        void Visit(DefaultAggregation defaultAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="dateHistogramAggregation">The Bucket Aggregations to visit.</param>
        void Visit(DateHistogramAggregation dateHistogramAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="termsAggregation">The Bucket Aggregations to visit.</param>
        void Visit(TermsAggregation termsAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="rangeAggregation">The Range aggregation to visit.</param>
        void Visit(RangeAggregation rangeAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="histogramAggregation">The Bucket Aggregations to visit.</param>
        void Visit(HistogramAggregation histogramAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="avgAggregation">The Average Aggregations to visit.</param>
        void Visit(AverageAggregation avgAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="cardinalityAggregation">The Cardinality Aggregations to visit.</param>
        void Visit(CardinalityAggregation cardinalityAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="extendedStatsAggregation">The Extended Stats Aggregations to visit.</param>
        void Visit(ExtendedStatsAggregation extendedStatsAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="minAggregation">The Min Aggregations to visit.</param>
        void Visit(MinAggregation minAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="maxAggregation">The Max Aggregations to visit.</param>
        void Visit(MaxAggregation maxAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="sumAggregation">The Sum Aggregations to visit.</param>
        void Visit(SumAggregation sumAggregation);

        /// <summary>
        /// Accepts a range expression, and builds the Kusto query.
        /// </summary>
        /// <param name="rangeAggregationExpression">The range aggregation expression.</param>
        void Visit(RangeAggregationExpression rangeAggregationExpression);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="percentileAggregation">The Percentile Aggregation to visit.</param>
        void Visit(PercentileAggregation percentileAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="topHitsAggregation">The Percentile Aggregation to visit.</param>
        void Visit(TopHitsAggregation topHitsAggregation);

        /// <summary>
        /// Accepts a query string clause, parses the phrase to a lucene query, and builds a Kusto query based on the lucene query.
        /// </summary>
        /// <param name="queryStringClause">The query string clause.</param>
        void Visit(QueryStringClause queryStringClause);

        /// <summary>
        /// Accepts a filters aggregation, and builds the Kusto query.
        /// </summary>
        /// <param name="filtersAggregation">The filters aggregation expression.</param>
        void Visit(FiltersAggregation filtersAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="dateRangeAggregation">The Range aggregation to visit.</param>
        void Visit(DateRangeAggregation dateRangeAggregation);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto query.
        /// </summary>
        /// <param name="dateRangeAggregationExpression">The range aggregation expression.</param>
        void Visit(DateRangeAggregationExpression dateRangeAggregationExpression);

        /// <summary>
        /// Accepts a given visitable object and builds a Kusto single doc query.
        /// </summary>
        /// <param name="documentIds">The DocumentIds object to visit.</param>
        void Visit(DocumentIds documentIds);
    }
}
