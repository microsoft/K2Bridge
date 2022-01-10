// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.JsonConverters;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using Newtonsoft.Json;

    /// <summary>
    /// Bucket aggregation that creates one bucket per filter query.
    /// </summary>
    internal class FiltersAggregation : BucketAggregation
    {
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        [JsonProperty("filters")]
        public Dictionary<string, FiltersBoolQuery> Filters { get; set; }

        /// <inheritdoc/>
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
