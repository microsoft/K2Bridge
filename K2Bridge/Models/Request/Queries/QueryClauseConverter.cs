// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class QueryClauseConverter : ReadOnlyJsonConverter
    {
        /// <summary>
        /// Read the given json and returns an object.
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

            // if we know this is a Bool Clause, deserialize accordingly
            if (objectType == typeof(BoolQuery))
            {
                return this.DeserializeBoolQuery(jo, serializer);
            }

            // if its an 'inner' bool, the type might indicate IQuery, but in fact its a bool
            if (((JProperty)jo.First).Name == "bool")
            {
                var body = ((JProperty)jo.First).Value;
                return this.DeserializeBoolQuery(body, serializer);
            }

            // Its really a leaf
            var leaf = jo.ToObject<ILeafClause>(serializer);
            return leaf;
        }

        private BoolQuery DeserializeBoolQuery(JToken token, JsonSerializer serializer)
        {
            return new BoolQuery
            {
                Must = this.TokenToIQueryClauseList(token["must"], serializer),
                MustNot = this.TokenToIQueryClauseList(token["must_not"], serializer),
                Should = this.TokenToIQueryClauseList(token["should"], serializer),
                ShouldNot = this.TokenToIQueryClauseList(token["should_not"], serializer),
            };
        }

        private IEnumerable<IQuery> TokenToIQueryClauseList(JToken token, JsonSerializer serializer)
        {
            return token != null ? token.ToObject<List<IQuery>>(serializer) : new List<IQuery>();
        }
    }
}
