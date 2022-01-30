// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace K2Bridge.Visitors.Aggregations.Helpers;

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
    /// Gets or sets the order by expression.
    /// </summary>
    public string OrderByExpression { get; set; }

    /// <summary>
    /// Gets or sets the limit expression.
    /// </summary>
    public string LimitExpression { get; set; }

    /// <summary>
    /// Gets or sets the project away expression.
    /// </summary>
    public string ProjectAwayExpression { get; set; }

    /// <summary>
    /// Gets or sets the metadata.
    /// </summary>
    public Dictionary<string, List<string>> Metadata { get; set; }
}
