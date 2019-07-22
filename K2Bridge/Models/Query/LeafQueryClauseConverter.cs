namespace K2Bridge
{
    using System;
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

                default:
                    return null;
            }
        }
    }
}
