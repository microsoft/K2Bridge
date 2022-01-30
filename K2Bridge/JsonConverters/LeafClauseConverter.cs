// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.JsonConverters;

using System;
using K2Bridge.JsonConverters.Base;
using K2Bridge.Models.Request.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// A converter able to deserialize a query leaf element from an Elasticsearch query to <see cref="ILeafClause"/>.
/// The actual type returned will change based on the actual leaf query (see implementation of the interface).
/// </summary>
internal class LeafClauseConverter : ReadOnlyJsonConverter
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

        return first.Name switch
        {
            "exists" => first.Value.ToObject<ExistsClause>(serializer),
            "match_phrase" => first.Value.ToObject<MatchPhraseClause>(serializer),
            "range" => first.Value.ToObject<RangeClause>(serializer),
            "query_string" => first.ToObject<QueryStringClause>(serializer),
            "bool" => ((JProperty)jo.First).Value.ToObject<BoolQuery>(serializer),
            _ => null,
        };
    }
}
