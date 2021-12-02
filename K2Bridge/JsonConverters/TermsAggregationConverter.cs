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
    /// A converter able to deserialize the date_histogram element in an Elasticsearch query to <see cref="TermsAggregation"/>.
    /// </summary>
    internal class TermsAggregationConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var obj = new TermsAggregation
            {
                FieldName = (string)jo["field"],
            };

            int? size = (int)jo["size"];
            if (size != null)
            {
                obj.Size = (int)size;
            }

            var order = jo["order"];
            if (order != null && order.First != null)
            {
                obj.SortFieldName = ((JProperty)order.First).Name;
                obj.SortOrder = (string)((JProperty)order.First).Value;
            }

            return obj;
        }
    }
}
