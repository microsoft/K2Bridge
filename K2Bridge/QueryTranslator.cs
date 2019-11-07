namespace K2Bridge
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    internal class QueryTranslator : ITranslator
    {
        private readonly IVisitor visitor;

        public QueryTranslator(IVisitor visitor) => this.visitor = visitor;

        public QueryData Translate(string header, string query)
        {
            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);

            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);
            elasticSearchDSL.IndexName = headerDictionary["index"];

            elasticSearchDSL.HighlightText = new Dictionary<string, string>();
            var qs = elasticSearchDSL.Query.Bool.Must.GetEnumerator();

            while (qs.MoveNext()) {
                if (qs.Current == null) {
                    continue;
                }

                if (qs.Current is QueryStringQuery) {
                    var q = (QueryStringQuery)qs.Current;
                    elasticSearchDSL.HighlightText.Add("*", q.Phrase);
                } else if (qs.Current is MatchPhraseQuery) {
                    var q = (MatchPhraseQuery)qs.Current;
                    elasticSearchDSL.HighlightText.Add(q.FieldName, q.Phrase);
                }
            }

            elasticSearchDSL.Accept(this.visitor);
            var queryData = new QueryData(elasticSearchDSL.KQL, elasticSearchDSL.IndexName, elasticSearchDSL.HighlightText);

            if (elasticSearchDSL.Highlight != null && elasticSearchDSL.Highlight.PreTags.Count > 0) {
                queryData.HighlightPreTag = elasticSearchDSL.Highlight.PreTags[0];
                queryData.HighlightPostTag = elasticSearchDSL.Highlight.PostTags[0];
            }

            return queryData;
        }
    }
}
