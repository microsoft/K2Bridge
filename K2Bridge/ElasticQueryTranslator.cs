﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using K2Bridge.Models;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Telemetry;
    using K2Bridge.Visitors;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// ElasticQueryTranslator provides the functionality for translating a Kibana query into Kusto query.
    /// </summary>
    internal class ElasticQueryTranslator : ITranslator
    {
        private readonly IVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticQueryTranslator"/> class.
        /// </summary>
        /// <param name="visitor">The visitor to accept the translation request.</param>
        /// <param name="logger">Logger.</param>
        public ElasticQueryTranslator(IVisitor visitor, ILogger<ElasticQueryTranslator> logger)
        {
            this.visitor = visitor;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        /// <summary>
        /// Translate a given Data request into QueryData.
        /// </summary>
        /// <param name="header">A header.</param>
        /// <param name="query">A query.</param>
        /// <returns>A <see cref="QueryData"/>.</returns>
        /// <exception cref="TranslateException">Throws a TranslateException on error.</exception>
        public QueryData TranslateData(string header, string query)
        {
            Ensure.IsNotNullOrEmpty(header, nameof(header));

            try
            {
                Logger.LogDebug("Translate data params: header:{@header}, query:{@query}", header, query.ToSensitiveData());

                // Prepare the esDSL object, except some fields such as the query field which will be built later
                var elasticSearchDsl = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);

                // deserialize the headers and extract the index name
                var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
                elasticSearchDsl.IndexName = headerDictionary["index"];

                elasticSearchDsl.HighlightText = new Dictionary<string, string>();

                Ensure.IsNotNull(elasticSearchDsl.Query, nameof(elasticSearchDsl.Query));
                Ensure.IsNotNull(elasticSearchDsl.Query.Bool, nameof(elasticSearchDsl.Query.Bool));
                Ensure.IsNotNull(elasticSearchDsl.Query.Bool.Must, nameof(elasticSearchDsl.Query.Bool.Must));

                if (elasticSearchDsl.Query.Bool.Filter.Any())
                {
                    // KQL in an experimental search syntax in Kibana that is turned on in version 7 but also available in version 6.
                    // One can set it with option "search:queryLanguage" to "Lucene". More info: https://www.elastic.co/guide/en/kibana/current/advanced-options.html.
                    Logger.LogWarning("Query includes a filter element indicating Kibana is working in KQL syntax, which is not supported yet. You should search with Lucene syntax instead.");
                }

                foreach (var element in elasticSearchDsl.Query.Bool.Must)
                {
                    switch (element)
                    {
                        case QueryStringClause queryStringClause:
                            elasticSearchDsl.HighlightText.Add("*", queryStringClause.Phrase);
                            break;
                        case MatchPhraseClause matchPhraseClause:
                            elasticSearchDsl.HighlightText.Add(matchPhraseClause.FieldName, matchPhraseClause.Phrase);
                            break;
                    }
                }

                var sortFields = new List<string>();
                elasticSearchDsl.Sort?.ForEach(clause =>
                {
                    sortFields.Add(clause.FieldName);
                });

                var docValueFields = new List<string>();
                elasticSearchDsl.DocValueFields?.ForEach(item =>
                {
                    docValueFields.Add(item.Field);
                });

                // Use the visitor and build the KustoQL string from the esDSL object
                elasticSearchDsl.Accept(visitor);
                var queryData = new QueryData(
                    elasticSearchDsl.KustoQL,
                    elasticSearchDsl.IndexName,
                    sortFields,
                    docValueFields,
                    elasticSearchDsl.HighlightText);

                if (elasticSearchDsl.Highlight != null)
                {
                    Ensure.IsNotNullOrEmpty(elasticSearchDsl.Highlight.PreTags, nameof(elasticSearchDsl.Highlight.PreTags));
                    Ensure.IsNotNullOrEmpty(elasticSearchDsl.Highlight.PostTags, nameof(elasticSearchDsl.Highlight.PostTags));

                    queryData.HighlightPreTag = elasticSearchDsl.Highlight.PreTags[0];
                    queryData.HighlightPostTag = elasticSearchDsl.Highlight.PostTags[0];
                }

                return queryData;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute translate.");
                throw new TranslateException("Failed translating elasticsearch query", ex);
            }
        }

        /// <summary>
        /// Translate a given Single Document request into QueryData.
        /// </summary>
        /// <param name="header">A header.</param>
        /// <param name="query">A query.</param>
        /// <returns>A <see cref="QueryData"/>.</returns>
        /// <exception cref="TranslateException">Throws a TranslateException on error.</exception>
        public QueryData TranslateSingleDocument(string header, string query)
        {
            Ensure.IsNotNullOrEmpty(header, nameof(header));

            try
            {
                Logger.LogDebug("Translate single document params: header:{@header}, query:{@query}", header, query.ToSensitiveData());

                // Prepare the esDSL object, except some fields such as the query field which will be built later
                var singleDocumentDsl = JsonConvert.DeserializeObject<SingleDocumentDsl>(query);

                // deserialize the headers and extract the index name
                var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(header);
                singleDocumentDsl.IndexName = headerDictionary["index"];

                // elasticSearchDsl.HighlightText = new Dictionary<string, string>();
                Ensure.IsNotNull(singleDocumentDsl.SingleDocQuery, nameof(singleDocumentDsl.SingleDocQuery));
                Ensure.IsNotNull(singleDocumentDsl.SingleDocQuery.DocumentId, nameof(singleDocumentDsl.SingleDocQuery.DocumentId));
                Ensure.IsNotNull(singleDocumentDsl.SingleDocQuery.DocumentId.Id, nameof(singleDocumentDsl.SingleDocQuery.DocumentId.Id));

                var docValueFields = new List<string>();
                singleDocumentDsl.DocValueFields?.ForEach(item =>
                {
                    docValueFields.Add(item.Field);
                });

                // Use the visitor and build the KustoQL string from the esDSL object
                singleDocumentDsl.Accept(visitor);

                var queryData = new QueryData(
                    singleDocumentDsl.KustoQL,
                    singleDocumentDsl.IndexName,
                    null,
                    docValueFields,
                    null);

                return queryData;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute translate.");
                throw new TranslateException("Failed translating single document elasticsearch query", ex);
            }
        }
    }
}
