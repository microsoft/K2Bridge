// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations;
    using Newtonsoft.Json;

    /// <summary>
    /// This converter serializes <see cref="ValueAggregate"/> to Elasticsearh response json format.
    /// </summary>
    internal class ValueAggregateConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueAggregate = (ValueAggregate)value;

            writer.WriteStartObject();

            // To be alligned with elasticsearch behavior, null value must be serialized.
            writer.WritePropertyName("value");
            serializer.Serialize(writer, valueAggregate.Value);

            if (valueAggregate.ValueAsString is not null)
            {
                writer.WritePropertyName("value_as_string");
                serializer.Serialize(writer, valueAggregate.ValueAsString);
            }

            writer.WriteEndObject();
        }
    }
}