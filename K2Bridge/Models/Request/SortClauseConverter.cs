// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class SortClauseConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a SortClause object.
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

            var obj = new SortClause
            {
                FieldName = first.Name,
                Order = (string)first.First["order"],
            };

            return obj;
        }
    }
}
