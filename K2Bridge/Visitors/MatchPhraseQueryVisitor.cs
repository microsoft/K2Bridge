namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhraseQuery matchPhraseQuery)
        {
            // Must have a field name
            if ((matchPhraseQuery.FieldName == null) ||
                    string.IsNullOrEmpty(matchPhraseQuery.FieldName))
            {
                matchPhraseQuery.KQL = string.Empty;
                return;
            }

            matchPhraseQuery.KQL = $"{matchPhraseQuery.FieldName} == \"{matchPhraseQuery.Phrase}\"";
        }
    }
}
