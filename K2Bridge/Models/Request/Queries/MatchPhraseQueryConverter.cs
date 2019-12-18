// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class MatchPhraseQueryConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns a MatchPhraseQuery object
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

            if (first.First.GetType() == typeof(JObject))
            {
                var obj = new MatchPhraseQuery
                {
                    FieldName = first.Name,
                    Phrase = (string)first.First["query"],
                };
                return obj;
            }
            else
            {
                var obj = new MatchPhraseQuery
                {
                    FieldName = first.Name,
                    Phrase = (string)((JValue)first.First).Value,
                };
                return obj;
            }
        }
    }
}
