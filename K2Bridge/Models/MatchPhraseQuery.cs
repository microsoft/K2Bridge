namespace K2Bridge
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(MatchPhraseQueryConverter))]
    internal class MatchPhraseQuery : LeafQueryClause, IVisitable
    {
        public string FieldName { get; set; }

        public string Phrase { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
