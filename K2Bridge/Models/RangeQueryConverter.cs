namespace K2Bridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class RangeQueryConverter : ReadOnlyJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RangeQuery);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            RangeQuery rangeQuery = new RangeQuery
            {
                FieldName = first.Name,
                GTEValue = (long)first.First["gte"],
                LTEValue = (long)first.First["lte"],
            };

            return rangeQuery;
        }
    }
}
