namespace K2Bridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class MatchPhraseQueryConverter : ReadOnlyJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MatchPhraseQuery);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            MatchPhraseQuery matchPhraseQuery = new MatchPhraseQuery
            {
                FieldName = first.Name,
                Phrase = (string)first.First["query"],
            };
            return matchPhraseQuery;
        }
    }
}
