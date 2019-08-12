namespace K2Bridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class SortClauseConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            var obj = new SortClause
            {
                FieldName = first.Name,
                Order = (string)first.First["order"],
            };

            return obj;
        }
    }
}
