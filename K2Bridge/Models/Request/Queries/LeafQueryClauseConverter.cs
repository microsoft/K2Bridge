// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LeafQueryClauseConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            switch (first.Name)
            {
                case "exists":
                    return first.Value.ToObject<ExistsQuery>(serializer);

                case "match_phrase":
                    return first.Value.ToObject<MatchPhraseQuery>(serializer);

                case "range":
                    return first.Value.ToObject<RangeQuery>(serializer);

                case "query_string":
                    return first.ToObject<QueryStringQuery>(serializer);

                case "bool":
                    return ((JProperty)jo.First).Value.ToObject<BoolClause>(serializer);

                default:
                    return null;
            }
        }
    }
}