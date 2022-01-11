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
            var primary = jo.Children<JProperty>().FirstOrDefault(x => x.Name != "aggs");

            obj.PrimaryAggregation = GetPrimaryAggregation(primary, reader, serializer);
            obj.SubAggregations = jo["aggs"]?.ToObject<AggregationDictionary>(serializer) ?? new AggregationDictionary();

            return obj;
        }

        private Aggregation GetPrimaryAggregation(JProperty property, JsonReader reader, JsonSerializer serializer)
        {
            Aggregation aggregation = null;

            if (property != null)
            {
                aggregation = property.Name switch
                {
                    "date_histogram" => property.Value.ToObject<DateHistogramAggregation>(serializer),
                    "terms" => property.Value.ToObject<TermsAggregation>(serializer),
                    "range" => property.Value.ToObject<RangeAggregation>(serializer),
                    "filters" => property.Value.ToObject<FiltersAggregation>(serializer),
                    "date_range" => property.Value.ToObject<DateRangeAggregation>(serializer),
                    "avg" => property.Value.ToObject<AverageAggregation>(serializer),
                    "cardinality" => property.Value.ToObject<CardinalityAggregation>(serializer),
                    "min" => property.Value.ToObject<MinAggregation>(serializer),
                    "max" => property.Value.ToObject<MaxAggregation>(serializer),
                    "sum" => property.Value.ToObject<SumAggregation>(serializer),
                    "percentiles" => property.Value.ToObject<PercentileAggregation>(serializer),
                    _ => aggregation,
                };
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
