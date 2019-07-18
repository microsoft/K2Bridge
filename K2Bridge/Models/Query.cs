namespace K2Bridge
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    class Query : KQLBase, IVisitable
    {
        [JsonProperty("match_phrase")]
        public MatchPhraseQuery MatchPhraseQuery { get; set; }

        [JsonProperty("range")]
        public MatchPhraseQuery RangeQuery { get; set; }

        [JsonProperty("bool")]
        public BoolClause Bool { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
