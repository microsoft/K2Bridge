// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class AggregationConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns an Aggregation object.
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

            var obj = new Aggregation
            {
                PrimaryAggregation = jo.ToObject<LeafAggregation>(serializer),
            };

            var aggsObject = jo["aggs"];
            if (aggsObject != null)
            {
                obj.SubAggregations =
                    aggsObject.ToObject<Dictionary<string, Aggregation>>(serializer);
            }
            else
            {
                obj.SubAggregations = new Dictionary<string, Aggregation>();
            }

            return obj;
        }
    }
}
