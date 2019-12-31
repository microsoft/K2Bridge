// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// ElasticQueryTranslator provides the functionality for translating a Kibana query into KQL
    /// </summary>
    internal class ElasticQueryTranslator : ITranslator
    {
        private readonly IVisitor visitor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="visitor">The visitor to accept the translation request</param>
        public ElasticQueryTranslator(IVisitor visitor) => this.visitor = visitor;

        /// <summary>
        /// Translate a given request into QueryData
        /// </summary>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryData Translate(string header, string query)
        {
            // Prepare the esDSL object, except some fields such as the query field which will be built later
            var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);

            // deserialize the headers and extract the index name
            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
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

            // Use the visitor and build the KQL string from the esDSL object
            elasticSearchDSL.Accept(this.visitor);
            var queryData = new QueryData(
                elasticSearchDSL.KQL,
                elasticSearchDSL.IndexName,
                elasticSearchDSL.HighlightText);

            if (elasticSearchDSL.Highlight != null && elasticSearchDSL.Highlight.PreTags.Count > 0) {
                queryData.HighlightPreTag = elasticSearchDSL.Highlight.PreTags[0];
                queryData.HighlightPostTag = elasticSearchDSL.Highlight.PostTags[0];
            }

            return queryData;
        }
    }
}
