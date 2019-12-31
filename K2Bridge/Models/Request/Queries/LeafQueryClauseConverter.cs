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
        /// <summary>
        /// Read the given json and returns an object
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

            switch (first.Name)
            {
                case "exists":
                    return first.Value.ToObject<Exists>(serializer);

                case "match_phrase":
                    return first.Value.ToObject<MatchPhrase>(serializer);

                case "range":
                    return first.Value.ToObject<Range>(serializer);

                case "query_string":
                    return first.ToObject<QueryString>(serializer);

                case "bool":
                    return ((JProperty)jo.First).Value.ToObject<BoolQuery>(serializer);

                default:
                    return null;
            }
        }
    }
}