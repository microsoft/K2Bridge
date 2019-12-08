// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafAggregationConverter))]
    internal abstract class LeafAggregation : KQLBase, IVisitable
    {
        public abstract void Accept(IVisitor visitor);
    }
}
