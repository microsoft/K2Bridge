namespace K2Bridge.Models.Aggregations
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LeafAggregationConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            LeafAggregation leafAggregation = null;
            switch (first.Name)
            {
                case "date_histogram":
                    leafAggregation = first.Value.ToObject<DateHistogram>(serializer);
                    break;

                case "avg":
                    leafAggregation = first.Value.ToObject<Avg>(serializer);
                    break;

                case "cardinality":
                    leafAggregation = first.Value.ToObject<Cardinality>(serializer);
                    break;
            }

            return leafAggregation;

        }
    }
}
