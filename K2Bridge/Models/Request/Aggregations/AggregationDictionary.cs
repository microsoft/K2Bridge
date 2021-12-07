// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes aggregation dictionary element in an Elasticsearch query.
    /// </summary>
    internal class AggregationDictionary : Dictionary<string, AggregationContainer>
    {
    }
}