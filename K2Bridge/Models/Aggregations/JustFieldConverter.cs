namespace K2Bridge.Models.Aggregations
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class JustFieldConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            dynamic obj = Activator.CreateInstance(objectType);
            obj.FieldName = (string)jo["field"];

            return obj;
        }
    }
}
