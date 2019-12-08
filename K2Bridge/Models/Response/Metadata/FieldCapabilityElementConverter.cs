// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Response.Metadata
{
    using System;
    using Newtonsoft.Json;

    internal class FieldCapabilityElementConverter : JsonConverter
    {
        private const string IsAggregatablePropetryName = "aggregatable";
        private const string IsSearchablePropetryName = "searchable";
        private const string TypePropetryName = "type";

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fieldCapabilityElement = value as FieldCapabilityElement;

            writer.WriteStartObject();
            writer.WritePropertyName(fieldCapabilityElement.Type);
            writer.WriteStartObject();
            writer.WritePropertyName(IsAggregatablePropetryName);
            serializer.Serialize(writer, fieldCapabilityElement.IsAggregatable);
            writer.WritePropertyName(IsSearchablePropetryName);
            serializer.Serialize(writer, fieldCapabilityElement.IsSearchable);
            writer.WritePropertyName(TypePropetryName);
            serializer.Serialize(writer, fieldCapabilityElement.Type);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
