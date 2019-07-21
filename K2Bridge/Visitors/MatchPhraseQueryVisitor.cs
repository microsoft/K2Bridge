namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhraseQuery matchPhraseQuery)
        {
            matchPhraseQuery.KQL = $"{matchPhraseQuery.FieldName} == \"{matchPhraseQuery.Phrase}\"";
        }
    }
}
