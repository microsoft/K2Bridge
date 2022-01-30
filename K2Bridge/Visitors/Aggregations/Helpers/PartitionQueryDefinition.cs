// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    internal class PartitionQueryDefinition
    {
        /// <summary>
        /// Gets or sets the aggregation expression.
        /// </summary>
        public string AggregationExpression { get; set; }

        /// <summary>
        /// Gets or sets the project expression.
        /// </summary>
        public string ProjectExpression { get; set; }

        /// <summary>
        /// Gets or sets the summarize expression.
        /// </summary>
        public string SummarizeExpression { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the partition query name.
        /// </summary>
        public string PartionQueryName { get; set; }
    }
}
