// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafClauseConverter))]
    internal interface ILeafClause : IQuery
    {
        string KustoQL { get; set; }
    }
}
