﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.JsonConverters;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// A single-value metrics aggregation that calculates an approximate count of distinct values.
    /// </summary>
    [JsonConverter(typeof(AggregationFieldConverter))]
    internal class CardinalityAggregation : MetricAggregation
    {
        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
