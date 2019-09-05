namespace K2Bridge.Models.Aggregations
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class AggregationConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var obj = new Aggregation
            {
                PrimaryAggregation = jo.ToObject<LeafAggregation>(serializer),
            };

            var aggsObject = jo["aggs"];
            if (aggsObject != null)
            {
                obj.SubAggregations = aggsObject.ToObject<Dictionary<string, Aggregation>>(serializer);
            }
            else
            {
                obj.SubAggregations = new Dictionary<string, Aggregation>();
            }

            return obj;
        }
    }
}
