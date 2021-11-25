// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.JsonConverters.Base;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize a 'field' property in an Elasticsearch query.
    /// </summary>
    internal class AggregationFieldConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            dynamic obj = Activator.CreateInstance(objectType);
            obj.FieldName = (string)jo["field"];

            return obj;
        }
    }
}
