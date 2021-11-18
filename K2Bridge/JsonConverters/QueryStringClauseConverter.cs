// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the query_string element in an Elasticsearch query to <see cref="QueryStringClause"/>.
    /// </summary>
    internal class QueryStringClauseConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jt = JToken.Load(reader);

            // we are setting the 'analyze_wildcard' and 'default_field' to defaults
            // as described here:
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-query-string-query.html#query-string-top-level-params
            var wildcard = jt.First["analyze_wildcard"];
            var defaultWildcard = jt.First["default_field"];
            var obj = new QueryStringClause
            {
                Phrase = (string)jt.First["query"],
                Wildcard = (wildcard != null) && (bool)wildcard,
                Default = (defaultWildcard != null) ? (string)defaultWildcard : "*",
            };

            return obj;
        }
    }
}
