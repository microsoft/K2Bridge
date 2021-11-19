// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Response;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the aggs element in an Elasticsearch query to <see cref="Aggregation"/>.
    /// </summary>
    internal class ResponseAggregationsConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var aggs = value as Aggregations;

            writer.WriteStartObject();
            writer.WritePropertyName(aggs.Parent);

            serializer.Serialize(writer, aggs.Collection);

            writer.WriteEndObject();
        }
    }
}
