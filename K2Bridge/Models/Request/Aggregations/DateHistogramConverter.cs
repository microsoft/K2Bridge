namespace K2Bridge.Models.Request.Aggregations
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class DateHistogramConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var obj = new DateHistogram
            {
                FieldName = (string)jo["field"],
                Interval = (string)jo["interval"],
            };

            return obj;
        }
    }
}
