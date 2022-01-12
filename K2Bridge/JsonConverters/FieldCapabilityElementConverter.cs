// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using K2Bridge.JsonConverters.Base;
    using K2Bridge.Models.Response.Metadata;
    using Newtonsoft.Json;

    internal class FieldCapabilityElementConverter : WriteOnlyJsonConverter
    {
        private const string IsAggregatablePropetryName = "aggregatable";
        private const string IsSearchablePropetryName = "searchable";
        private const string IsMetadataPropetryName = "metadata_field";

        private const string TypePropetryName = "type";

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
            writer.WritePropertyName(IsMetadataPropetryName);
            serializer.Serialize(writer, fieldCapabilityElement.IsMetadataField);
            writer.WritePropertyName(TypePropetryName);
            serializer.Serialize(writer, fieldCapabilityElement.Type);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
