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
    /// A converter able to deserialize the march_phrase element in an Elasticsearch query to <see cref="MatchPhraseClause"/>.
    /// </summary>
    internal class MatchPhraseClauseConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            if (first.First.GetType() == typeof(JObject))
            {
                var obj = new MatchPhraseClause
                {
                    FieldName = first.Name,
                    Phrase = (string)first.First["query"],
                };
                return obj;
            }
            else
            {
                var obj = new MatchPhraseClause
                {
                    FieldName = first.Name,
                    Phrase = (string)((JValue)first.First).Value,
                };
                return obj;
            }
        }
    }
}
