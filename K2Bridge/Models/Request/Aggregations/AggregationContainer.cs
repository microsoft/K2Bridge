// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Text.Json.Serialization;
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;

    [JsonConverter(typeof(AggregationContainerConverter))]
    internal class AggregationContainer : KustoQLBase, IVisitable
    {
        public Aggregation Primary { get; set; }

        public AggregationDictionary Aggregations { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
