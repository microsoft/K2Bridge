// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class SortClauseConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
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
