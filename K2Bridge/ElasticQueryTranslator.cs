// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// ElasticQueryTranslator provides the functionality for translating a Kibana query into KQL.
    /// </summary>
    internal class ElasticQueryTranslator : ITranslator
    {
        private readonly IVisitor Visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticQueryTranslator"/> class.
        /// </summary>
        /// <param name="visitor">The visitor to accept the translation request.</param>
        /// <param name="logger">Logger.</param>
        public ElasticQueryTranslator(IVisitor visitor, ILogger<ElasticQueryTranslator> logger)
        {
            Visitor = visitor;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Translate a given request into QueryData.
        /// </summary>
        /// <param name="header">A header.</param>
        /// <param name="query">A query.</param>
        /// <returns>A <see cref="QueryData"/>.</returns>
        public QueryData Translate(string header, string query)
        {
            try
            {
                Logger.LogDebug("Translate params: header:{@header}, query:{@query}", header, query);

                // Prepare the esDSL object, except some fields such as the query field which will be built later
                var elasticSearchDSL = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);

                // deserialize the headers and extract the index name
                var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
                elasticSearchDSL.IndexName = headerDictionary["index"];

                elasticSearchDSL.HighlightText = new Dictionary<string, string>();

                var qs = elasticSearchDSL.Query.Bool.Must.GetEnumerator();
                while (qs.MoveNext())
                {
                    if (qs.Current == null)
                    {
                        continue;
                    }

                    if (qs.Current is QueryStringClause queryStringClause)
                    {
                        elasticSearchDSL.HighlightText.Add("*", queryStringClause.Phrase);
                    }
                    else if (qs.Current is MatchPhraseClause matchPhraseClause)
                    {
                        elasticSearchDSL.HighlightText.Add(matchPhraseClause.FieldName, matchPhraseClause.Phrase);
                    }
                }

                // Use the visitor and build the KQL string from the esDSL object
                elasticSearchDSL.Accept(Visitor);
                var queryData = new QueryData(
                    elasticSearchDSL.KQL,
                    elasticSearchDSL.IndexName,
                    elasticSearchDSL.HighlightText);

                if (elasticSearchDSL.Highlight != null && elasticSearchDSL.Highlight.PreTags.Count > 0)
                {
                    queryData.HighlightPreTag = elasticSearchDSL.Highlight.PreTags[0];
                    queryData.HighlightPostTag = elasticSearchDSL.Highlight.PostTags[0];
                }

                return queryData;
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex, "Failed to execute translate.");
                throw;
            }
        }
    }
}