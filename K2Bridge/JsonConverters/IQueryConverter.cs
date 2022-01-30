// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters;

using System;
using System.Collections.Generic;
using K2Bridge.JsonConverters.Base;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// A converter able to deserialize the query element in an Elasticsearch query
/// Since the query element is a compound (vs. leaf) this can return
/// a <see cref="BoolQuery"/> or <see cref="ILeafClause"/> object.
/// </summary>
internal class IQueryConverter : ReadOnlyJsonConverter
{
    /// <inheritdoc/>
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);

        // if we know this is a Bool Clause, deserialize accordingly
        if (objectType == typeof(BoolQuery))
        {
            return DeserializeBoolQuery(jo, serializer);
        }

        // if its an 'inner' bool, the type might indicate IQuery, but in fact its a bool
        if (((JProperty)jo.First).Name == "bool")
        {
            var body = ((JProperty)jo.First).Value;
            return DeserializeBoolQuery(body, serializer);
        }

        // Its really a leaf
        return jo.ToObject<ILeafClause>(serializer);
    }

    private static IEnumerable<IQuery> TokenToIQueryClauseList(JToken token, JsonSerializer serializer)
    {
        return token != null ? token.ToObject<List<IQuery>>(serializer) : new List<IQuery>();
    }

    private static BoolQuery DeserializeBoolQuery(JToken token, JsonSerializer serializer)
    {
        return new BoolQuery
        {
            Must = TokenToIQueryClauseList(token["must"], serializer),
            MustNot = TokenToIQueryClauseList(token["must_not"], serializer),
            Should = TokenToIQueryClauseList(token["should"], serializer),
            ShouldNot = TokenToIQueryClauseList(token["should_not"], serializer),
            Filter = TokenToIQueryClauseList(token["filter"], serializer),
        };
    }
}
