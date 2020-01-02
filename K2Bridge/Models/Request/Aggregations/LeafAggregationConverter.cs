// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class LeafAggregationConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a LeafAggregation object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            LeafAggregation leafAggregation = null;
            switch (first.Name)
            {
                case "date_histogram":
                    leafAggregation = first.Value.ToObject<DateHistogramAggregation>(serializer);
                    break;

                case "avg":
                    leafAggregation = first.Value.ToObject<AvgAggregation>(serializer);
                    break;

                case "cardinality":
                    leafAggregation = first.Value.ToObject<CardinalityAggregation>(serializer);
                    break;
            }

            return leafAggregation;
        }
    }
}
