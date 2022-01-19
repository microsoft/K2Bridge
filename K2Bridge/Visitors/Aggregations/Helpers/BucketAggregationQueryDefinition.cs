// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;

    internal class BucketAggregationQueryDefinition
    {
        /// <summary>
        /// Gets or sets the extend expression.
        /// </summary>
        public string ExtendExpression { get; set; }

        /// <summary>
        /// Gets or sets the bucket expression.
        /// </summary>
        public string BucketExpression { get; set; }

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }
}