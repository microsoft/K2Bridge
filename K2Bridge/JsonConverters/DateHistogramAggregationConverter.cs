// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.Models.Request.Aggregations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the date_histogram element in an Elasticsearch query to <see cref="DateHistogramAggregation"/>.
    /// </summary>
    internal class DateHistogramAggregationConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var fixedInterval = jo["fixed_interval"];
            var calendarInterval = jo["calendar_interval"];
            var interval =
                fixedInterval != null ? (string)fixedInterval : (string)calendarInterval;
            var obj = new DateHistogramAggregation
            {
                FieldName = (string)jo["field"],
                Interval = interval,
            };

            return obj;
        }
    }
}
