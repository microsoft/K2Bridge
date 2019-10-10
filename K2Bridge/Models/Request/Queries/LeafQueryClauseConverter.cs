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
                    var match = first.Value.ToObject<MatchPhraseQuery>(serializer);
                    return match;

                case "range":
                    var range = first.Value.ToObject<RangeQuery>(serializer);
                    return range;

                case "query_string":
                    var search = jo.ToObject<QueryStringQuery>(serializer);
                    return search;

                default:
                    return null;
            }
        }
    }
}
