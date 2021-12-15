// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes aggregation container element in an Elasticsearch query.
    /// </summary>
    [JsonConverter(typeof(AggregationContainerConverter))]
    public class AggregationContainer : KustoQLBase, IVisitable
    {
        /// <summary>
        /// Gets or sets primary/most relevant aggregation.
        /// </summary>
        public Aggregation PrimaryAggregation { get; internal set; }

        /// <summary>
        /// Gets or sets sub aggregations.
        /// </summary>
        public AggregationDictionary SubAggregations { get; internal set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
