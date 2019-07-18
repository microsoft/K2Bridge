namespace K2Bridge
{
    partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Query query)
        {
            query.MatchPhraseQuery.Accept(this);
            query.KQL = $"where ({query.MatchPhraseQuery.KQL})";
        }
    }
}
