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
            JToken jt = JToken.Load(reader);

            var obj = new QueryStringQuery
            {
                Phrase = (string)jt.First["query"],
                Wildcard = (bool)jt.First["analyze_wildcard"],
                Default = (string)jt.First["default_field"],
            };

            return obj;
        }
    }
}
