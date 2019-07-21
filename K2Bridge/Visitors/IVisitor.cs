namespace K2Bridge
{
    internal interface IVisitor
    {
        void Visit(MatchPhraseQuery matchPhraseQuery);

        void Visit(Query query);

        void Visit(ElasticSearchDSL elasticSearchDSL);

        void Visit(RangeQuery rangeQuery);

        void Visit(BoolClause boolClause);
    }
}
