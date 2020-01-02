// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class QueryStringClauseConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a QueryStringClause object.
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
            JToken jt = JToken.Load(reader);

            var obj = new QueryStringClause
            {
                Phrase = (string)jt.First["query"],
                Wildcard = (bool)jt.First["analyze_wildcard"],
                Default = (string)jt.First["default_field"],
            };

            return obj;
        }
    }
}
