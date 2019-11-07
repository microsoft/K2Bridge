namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class RangeQueryConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            RangeQuery obj = new RangeQuery
            {
                FieldName = first.Name,
                GTEValue = first.First.Value<decimal?>("gte"),
                GTValue = first.First.Value<decimal?>("gt"),
                LTEValue = first.First.Value<decimal?>("lte"),
                LTValue = first.First.Value<decimal?>("lt"),
                Format = (string)first.First["format"],
            };

            return obj;
        }
    }
}
