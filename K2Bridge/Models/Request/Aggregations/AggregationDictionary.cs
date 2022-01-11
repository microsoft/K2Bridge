// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using System.Collections.Generic;
    using K2Bridge.Visitors;

    /// <summary>
    /// Describes aggregation dictionary element in an Elasticsearch query.
    /// </summary>
    internal class AggregationDictionary : Dictionary<string, AggregationContainer>, IVisitable
    {
        /// <summary>
        /// Gets or sets the translation of the query.
        /// </summary>
        public string KustoQL { get; set; }

        /// <summary>
        ///  Gets or sets the parent aggregation conatiner.
        /// </summary>
        public AggregationContainer Parent { get; set; }

        /// <inheritdoc/>
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}