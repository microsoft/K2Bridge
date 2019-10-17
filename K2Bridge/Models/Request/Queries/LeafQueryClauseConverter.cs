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
                case "match_phrase":
                    return first.Value.ToObject<MatchPhraseQuery>(serializer);

                case "range":
                    return first.Value.ToObject<RangeQuery>(serializer);

                case "query_string":
                    return jo.ToObject<QueryStringQuery>(serializer);

                case "bool":
                    return ((JProperty)jo.First).Value.ToObject<BoolClause>(serializer);

                default:
                    return null;
            }
        }
    }
}
