// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Queries
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class QueryClauseConverter : ReadOnlyJsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // if we know this is a Bool Clause, deserialize accordingly
            if (objectType == typeof(BoolClause))
            {
                return this.DeserializeBoolClause(jo, serializer);
            }

            // if its an 'inner' bool, the type might indicate IQueryClause, but in fact its a bool
            if (((JProperty)jo.First).Name == "bool")
            {
                var body = ((JProperty)jo.First).Value;
                return this.DeserializeBoolClause(body, serializer);
            }

            // Its really a leaf
            var leaf = jo.ToObject<ILeafQueryClause>(serializer);
            return leaf;
        }

        private BoolClause DeserializeBoolClause(JToken token, JsonSerializer serializer)
        {
            return new BoolClause
            {
                Must = this.TokenToIQueryClauseList(token["must"], serializer),
                MustNot = this.TokenToIQueryClauseList(token["must_not"], serializer),
                Should = this.TokenToIQueryClauseList(token["should"], serializer),
                ShouldNot = this.TokenToIQueryClauseList(token["should_not"], serializer),
            };
        }

        private IEnumerable<IQueryClause> TokenToIQueryClauseList(JToken token, JsonSerializer serializer)
        {
            return token != null ? token.ToObject<List<IQueryClause>>(serializer) : new List<IQueryClause>();
        }
    }
}
