namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
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
