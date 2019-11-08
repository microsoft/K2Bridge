namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(MatchPhraseQuery matchPhraseQuery)
        {
            // Must have a field name
            if (string.IsNullOrEmpty(matchPhraseQuery.FieldName))
            {
                throw new IllegalClauseException("FieldName must have a valid value");
            }

            matchPhraseQuery.KQL = $"{matchPhraseQuery.FieldName} == \"{matchPhraseQuery.Phrase}\"";
        }
    }
}
