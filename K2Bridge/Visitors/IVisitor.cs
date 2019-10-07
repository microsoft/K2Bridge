namespace K2Bridge
{
    using K2Bridge.Models.Request.Aggregations;

    internal interface IVisitor
    {
        void Visit(MatchPhraseQuery matchPhraseQuery);

        void Visit(Query query);

        void Visit(ElasticSearchDSL elasticSearchDSL);

        void Visit(RangeQuery rangeQuery);

        void Visit(BoolClause boolClause);

        void Visit(SortClause sortClause);

        void Visit(Aggregation aggregation);

        // Bucket Aggregations
        void Visit(DateHistogram dateHistogram);

        // Metric Aggregations
        void Visit(Avg avg);

        void Visit(Cardinality cardinality);

        void Visit(QueryStringQuery queryStringQuery);
    }
}
