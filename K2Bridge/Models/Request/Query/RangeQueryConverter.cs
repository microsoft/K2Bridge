namespace K2Bridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class RangeQueryConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;
            RangeQuery obj;

            // there are more then 2 cases, will add them later
            if (first.Name.Equals("timestamp"))
            {
                obj = new RangeQuery
                {
                    FieldName = first.Name,
                    GTEValue = (long)first.First["gte"],
                    LTEValue = (long)first.First["lte"],
                    Format = (string)first.First["format"],
                };
            }
            else
            {
                obj = new RangeQuery
                {
                    FieldName = first.Name,
                    GTEValue = (long)first.First["gte"],
                    LTValue = (long)first.First["lt"],
                };
            }

            return obj;
        }
    }
}
