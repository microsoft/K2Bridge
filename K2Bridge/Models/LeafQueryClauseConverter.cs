namespace K2Bridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LeafQueryClauseConverter : ReadOnlyJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LeafQueryClause);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            if (first.Name == "match_phrase")
            {
                // TODO: check if there's a way to have a type variable to write this code just once.
                var match = first.Value.ToObject<MatchPhraseQuery>(serializer);
                return match;
            }

            if (first.Name == "range")
            {
                var range = first.Value.ToObject<RangeQuery>(serializer);
                return range;
            }

            // TODO: add a log for unrecognized leaf query type that wasn't deser.
            return null;
        }
    }
}
