// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the sort element in an Elasticsearch query to <see cref="SortClause"/>.
    /// </summary>
    internal class SortClauseConverter : ReadOnlyJsonConverter
    {
        /// <inheritdoc/>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            var obj = new SortClause
            {
                FieldName = first.Name,
                Order = (string)first.First["order"],
            };

            return obj;
        }
    }
}
