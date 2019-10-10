namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhraseQuery matchPhraseQuery)
        {
            matchPhraseQuery.KQL = $"{matchPhraseQuery.FieldName} == \"{matchPhraseQuery.Phrase}\"";
        }
    }
}
