// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the aggs element in an Elasticsearch query to <see cref="Aggregation"/>.
    /// </summary>
    internal class AggregationConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var path = reader.Path.Split(".");
            var parent = path[path.Length - 1];

            var obj = new Aggregation
            {
                PrimaryAggregation = jo.ToObject<LeafAggregation>(serializer),
            };

            if (obj.PrimaryAggregation != null)
            {
                obj.PrimaryAggregation.FieldAlias = $"_{parent}";
            }

            var aggs = jo["aggs"];
            if (aggs != null)
            {
                obj.SubAggregations =
                    aggs.ToObject<Dictionary<string, Aggregation>>(serializer);
            }
            else
            {
                obj.SubAggregations = new Dictionary<string, Aggregation>();
            }

            return obj;
        }
    }
}
