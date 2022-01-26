// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System.Globalization;
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Aggregations;
    using Newtonsoft.Json;

    /// <summary>
    /// This converter serializes <see cref="PercentileAggregate"/> to Elasticsearh response json format.
    /// </summary>
    internal class PercentileAggregateConverter : WriteOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var percentileAggregate = (PercentileAggregate)value;

            writer.WriteStartObject();
            writer.WritePropertyName("values");

            if (percentileAggregate.Keyed)
            {
                writer.WriteStartObject();

                foreach (var percentileItem in percentileAggregate.Values)
                {
                    var key = percentileItem.Percentile.ToString("F1", CultureInfo.InvariantCulture);

                    // To be alligned with elasticsearch behavior, null value must be serialized.
                    writer.WritePropertyName(key);
                    serializer.Serialize(writer, percentileItem.Value);

                    if (percentileItem.ValueAsString is not null)
                    {
                        writer.WritePropertyName($"{key}_as_string");
                        serializer.Serialize(writer, percentileItem.ValueAsString);
                    }
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartArray();

                foreach (var percentileItem in percentileAggregate.Values)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("key");
                    serializer.Serialize(writer, percentileItem.Percentile);

                    // To be alligned with elasticsearch behavior, null value must be serialized.
                    writer.WritePropertyName("value");
                    serializer.Serialize(writer, percentileItem.Value);

                    if (percentileItem.ValueAsString is not null)
                    {
                        writer.WritePropertyName("value_as_string");
                        serializer.Serialize(writer, percentileItem.ValueAsString);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}