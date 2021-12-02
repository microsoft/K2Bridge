// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize an aggregation leaf element in an Elasticsearch query to <see cref="LeafAggregation"/>.
    /// The actual type returned will change based on the actual leaf aggregation (see implementation of the interface).
    /// </summary>
    internal class LeafAggregationConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            LeafAggregation leafAggregation = null;
            switch (first.Name)
            {
                case "date_histogram":
                    leafAggregation = first.Value.ToObject<DateHistogramAggregation>(serializer);
                    break;

                case "terms":
                    leafAggregation = first.Value.ToObject<TermsAggregation>(serializer);
                    break;

                case "avg":
                    leafAggregation = first.Value.ToObject<AvgAggregation>(serializer);
                    break;

                case "cardinality":
                    leafAggregation = first.Value.ToObject<CardinalityAggregation>(serializer);
                    break;

                case "max":
                    leafAggregation = first.Value.ToObject<MaxAggregation>(serializer);
                    break;

                case "min":
                    leafAggregation = first.Value.ToObject<MinAggregation>(serializer);
                    break;

                case "sum":
                    leafAggregation = first.Value.ToObject<SumAggregation>(serializer);
                    break;
            }

            return leafAggregation;
        }
    }
}
