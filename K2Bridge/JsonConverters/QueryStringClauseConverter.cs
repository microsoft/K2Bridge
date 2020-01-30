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
            JToken jt = JToken.Load(reader);

            var obj = new QueryStringClause
            {
                Phrase = (string)jt.First["query"],
                Wildcard = (bool)jt.First["analyze_wildcard"],
                Default = (string)jt.First["default_field"],
            };

            return obj;
        }
    }
}
