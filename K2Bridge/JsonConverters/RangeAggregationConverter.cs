// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the date_histogram element in an Elasticsearch query to <see cref="TermsAggregation"/>.
    /// </summary>
    internal class RangeAggregationConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var obj = new RangeAggregation
            {
                FieldName = (string)jo["field"],
                IsKeyed = jo.ContainsKey("keyed") ? (bool)jo["keyed"] : false,
                Ranges = new List<RangeAggregationExpression>(),
            };

            var ranges = jo["ranges"];
            foreach (var rangeToken in ranges.Children())
            {
                var range = (JObject)rangeToken;
                var rangeExpr = new RangeAggregationExpression
                {
                    From = range.ContainsKey("from") ? (double)range["from"] : null,
                    To = range.ContainsKey("to") ? (double)range["to"] : null,
                    Key = range.ContainsKey("key") ? (string)range["key"] : null,
                };
                obj.Ranges.Add(rangeExpr);
            }

            return obj;
        }
    }
}
