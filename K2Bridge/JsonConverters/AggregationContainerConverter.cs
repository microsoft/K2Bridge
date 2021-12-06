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

    /// <summary>
    /// A converter able to deserialize the aggregation element in an Elasticsearch query to <see cref="AggregationContainer"/>.
    /// </summary>
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
            var primary = jo.Children<JProperty>().Where(x => x.Name != "aggs").FirstOrDefault();

            obj.PrimaryAggregation = GetPrimaryAggregation(primary, reader, serializer);
            obj.SubAggregations = jo["aggs"]?.ToObject<AggregationDictionary>(serializer) ?? new AggregationDictionary();

            return obj;
        }

        private Aggregation GetPrimaryAggregation(JProperty property, JsonReader reader, JsonSerializer serializer)
        {
            Aggregation aggregation = null;

            if (property != null)
            {
                switch (property.Name)
                {
                    case "date_histogram":
                        aggregation = property.Value.ToObject<DateHistogramAggregation>(serializer);
                        break;

                    case "terms":
                        aggregation = property.Value.ToObject<TermsAggregation>(serializer);
                        break;

                    case "avg":
                        aggregation = property.Value.ToObject<AverageAggregation>(serializer);
                        break;

                    case "cardinality":
                        aggregation = property.Value.ToObject<CardinalityAggregation>(serializer);
                        break;

                    case "max":
                        aggregation = property.Value.ToObject<MaxAggregation>(serializer);
                        break;

                    case "sum":
                        aggregation = property.Value.ToObject<SumAggregation>(serializer);
                        break;
                }
            }

            var key = reader.Path.Split(".")[^1];
            if (aggregation != null && !string.IsNullOrWhiteSpace(key))
            {
                aggregation.Key = key;
            }

            return aggregation;
        }
    }
}
