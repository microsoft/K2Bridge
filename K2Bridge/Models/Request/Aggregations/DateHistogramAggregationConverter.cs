// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Aggregations
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class DateHistogramAggregationConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a DateHistogramAggregation object
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

            var obj = new DateHistogramAggregation
            {
                FieldName = (string)jo["field"],
                Interval = (string)jo["interval"],
            };

            return obj;
        }
    }
}
