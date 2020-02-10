// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.KustoConnector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using K2Bridge.Models;
    using Lucene.Net.Analysis;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search.Highlight;
    using Microsoft.Extensions.Logging;
    using static System.Globalization.CultureInfo;
    using LuceneVersion = Lucene.Net.Util.Version;

    /// <summary>
    /// This class tranforms text by adding pre and post tags to mark highlighted entried.
    /// It works using an input query which holds the search entries and the pre/post tags
    /// which are used during tranformation.
    /// </summary>
    internal class LuceneHighlighter : IDisposable
    {
        private const string Default = "default";
        private readonly bool isHighlight = false;

        /// <summary>
        /// Lucene analyzer.
        /// </summary>
        private readonly Lazy<Analyzer> analyzer = new Lazy<Analyzer>(() => new WordAnalyzer());

        /// <summary>
        /// Highlighters computed for this query.
        /// </summary>
        private readonly Lazy<IDictionary<string, Highlighter>> highlighters;

        /// <summary>
        /// Query which highlighters are based on.
        /// </summary>
        private readonly QueryData query;
        private readonly ILogger logger;
        private bool disposedValue = false;

        public LuceneHighlighter(QueryData query, ILogger logger)
        {
            this.query = query;
            this.logger = logger;

            // Skipping highlight if the query's HighlightText dictionary is empty or if pre/post tags are empty.
            isHighlight = query.HighlightText != null && !string.IsNullOrEmpty(query.HighlightPreTag) && !string.IsNullOrEmpty(query.HighlightPostTag);
            highlighters = new Lazy<IDictionary<string, Highlighter>>(() => MakeHighlighters(analyzer.Value, query));
            logger.LogInformation($"Lucene highlighter is enabled: {isHighlight}");
        }

        /// <summary>
        /// Returns a highlighted transformation of the value.
        /// </summary>
        /// <param name="columnName">Field name.</param>
        /// <param name="value">Field Value.</param>
        /// <returns>a highlight-tagged version of the input value, if highlight is on and value is not empty.</returns>
        public string GetHighlightedValue(string columnName, object value)
        {
            try
            {
                if (!isHighlight || value == null)
                {
                    return string.Empty;
                }

                var stringValue = value.ToString();
                Highlighter highlighter = null;
                if (query.HighlightText.ContainsKey("*") && highlighters.Value.ContainsKey("*"))
                {
                    highlighter = highlighters.Value["*"];
                }

                if (query.HighlightText.ContainsKey(columnName) && highlighters.Value.ContainsKey(columnName))
                {
                    highlighter = highlighters.Value[columnName];
                }

                if (highlighter == null)
                {
                    return string.Empty;
                }

                return highlighter.GetBestFragment(analyzer.Value, columnName, stringValue);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failure getting highlighted value for {columnName}.");
                return null;
            }
        }

        /// <summary>
        /// Implement dispose pattern.
        /// </summarTy>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement dispose pattern.
        /// </summarTy>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (analyzer.IsValueCreated)
                    {
                        analyzer.Value.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Creates a dicionary of highlighters based on the query's HighlightText dictionary.
        /// </summarTy>
        /// <param name="analyzer">A lucene word analyzer.</param>
        /// <param name="query">The query.</param>
        /// <returns>a dictionary of searched tokens and their highlighters.</returns>
        private IDictionary<string, Highlighter> MakeHighlighters(Analyzer analyzer, QueryData query)
        {
            var parser = new QueryParser(LuceneVersion.LUCENE_30, Default, analyzer);
            return query.HighlightText
                .Select(kv => (key: kv.Key, highlighter: MakeValueHighlighter(parser, kv.Value, query.HighlightPreTag, query.HighlightPostTag)))
                .Where(kv => kv.highlighter != null)
                .ToDictionary(x => x.key, x => x.highlighter);
        }

        /// <summary>
        /// Creates a single highlighter.
        /// </summarTy>
        /// <param name="parser">A lucene parser.</param>
        /// <param name="value">The value which was searched.</param>
        /// <param name="highlightPreTag">Pre match tag.</param>
        /// <param name="highlightPostTag">Post match taf.</param>
        /// <returns>a highlighter.</returns>
        private Highlighter MakeValueHighlighter(QueryParser parser, string value, string highlightPreTag, string highlightPostTag)
        {
            // With lucene-net 3.0.3 some queries are not supported, for instance query such as "*someterm" (prefix is wildcard).
            // These queries throw exception when calling QueryParser.Parse(string value) regarding use of configuration manager
            // which is not supported in net core. see bug https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1658
            // these terms are discarded during the following creation of highlighter.
            try
            {
                var luceneQuery = parser.Parse(value);
                var scorer = new QueryScorer(luceneQuery);
                var formatter = new SimpleHTMLFormatter(highlightPreTag, highlightPostTag);
                return new Highlighter(formatter, scorer)
                    {
                        TextFragmenter = new SimpleSpanFragmenter(scorer, int.MaxValue),
                        MaxDocCharsToAnalyze = int.MaxValue,
                    };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failure creating highlighters for {value}");
                return null;
            }
        }

        private sealed class WordAnalyzer : Analyzer
        {
            public override TokenStream TokenStream(string fieldName, TextReader reader)
            {
                return new WordTokenizer(reader);
            }

            private sealed class WordTokenizer : CharTokenizer
            {
                public WordTokenizer(TextReader input)
                    : base(input)
                {
                }

                protected override bool IsTokenChar(char c)
                {
                    return char.IsLetterOrDigit(c) || c.Equals('_');
                }

                protected override char Normalize(char c)
                {
                    return char.ToLower(c, InvariantCulture);
                }
            }
        }
    }
}