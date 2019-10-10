namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class QueryStringQueryConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            var obj = new QueryStringQuery
            {
                FieldName = first.Name,
                Phrase = (string)first.First["query"],
                Wildcard = (bool)first.First["analyze_wildcard"],
                Default = (string)first.First["default_field"],
            };

            return obj;
        }
    }
}