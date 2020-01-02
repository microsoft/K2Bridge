// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class RangeClauseConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a RangeClause object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var first = (JProperty)jo.First;

            RangeClause obj = new RangeClause
            {
                FieldName = first.Name,
                GTEValue = first.First.Value<decimal?>("gte"),
                GTValue = first.First.Value<decimal?>("gt"),
                LTEValue = first.First.Value<decimal?>("lte"),
                LTValue = first.First.Value<decimal?>("lt"),
                Format = (string)first.First["format"],
            };

            return obj;
        }
    }
}
