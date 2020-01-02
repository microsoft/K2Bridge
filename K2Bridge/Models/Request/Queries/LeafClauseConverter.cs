// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LeafClauseConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns an object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            return first.Name switch
            {
                "exists" => first.Value.ToObject<ExistsClause>(serializer),
                "match_phrase" => first.Value.ToObject<MatchPhraseClause>(serializer),
                "range" => first.Value.ToObject<RangeClause>(serializer),
                "query_string" => first.ToObject<QueryStringClause>(serializer),
                "bool" => ((JProperty)jo.First).Value.ToObject<BoolQuery>(serializer),
                _ => null,
            };
        }
    }
}