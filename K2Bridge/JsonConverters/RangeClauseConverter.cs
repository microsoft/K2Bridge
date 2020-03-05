// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters
{
    using System;
    using K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A converter able to deserialize the range element in an Elasticsearch query to <see cref="RangeClause"/>.
    /// </summary>
    internal class RangeClauseConverter : ReadOnlyJsonConverter
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

            var obj = new RangeClause
            {
                FieldName = first.Name,
                GTEValue = first.First.Value<string>("gte"),
                GTValue = first.First.Value<string>("gt"),
                LTEValue = first.First.Value<string>("lte"),
                LTValue = first.First.Value<string>("lt"),
                Format = (string)first.First["format"],
            };

            return obj;
        }
    }
}
