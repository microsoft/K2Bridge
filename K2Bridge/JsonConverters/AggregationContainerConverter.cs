// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using System.Linq;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class AggregationContainerConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var obj = new AggregationContainer();
            var primary = jo.Children<JProperty>().Where(x => x.Name != "aggs").First();

            obj.Primary = GetPrimaryAggregation(primary, serializer);
            obj.Aggregations = jo["aggs"]?.ToObject<AggregationDictionary>(serializer);

            return obj;
        }

        private Aggregation GetPrimaryAggregation(JProperty property, JsonSerializer serializer)
        {
            Aggregation aggregation = null;
            switch (property.Name)
            {
                case "date_histogram":
                    aggregation = property.Value.ToObject<DateHistogramAggregation>(serializer);
                    break;

                case "avg":
                    aggregation = property.Value.ToObject<AverageAggregation>(serializer);
                    break;

                case "max":
                    aggregation = property.Value.ToObject<MaxAggregation>(serializer);
                    break;
            }

            return aggregation;
        }
    }
}