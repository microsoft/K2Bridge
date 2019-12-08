// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request
{
    using K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;

    [JsonConverter(typeof(QueryClauseConverter))]
    internal interface IQueryClause
    {
    }
}
