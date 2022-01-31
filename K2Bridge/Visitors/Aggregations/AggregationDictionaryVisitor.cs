// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K2Bridge.Models.Request.Aggregations;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Utils;

/// <content>
/// A visitor for the root <see cref="AggregationDictionary"/> element.
/// </content>
internal partial class ElasticSearchDSLVisitor : IVisitor
{
    /// <summary>
    /// Gets a list of sub query names
    /// Each time a new sub query is created, it's name is added to the list
    /// The last name of the stack is returned as aggs table.
    /// </summary>
    internal List<string> SubQueriesStack { get; } = new List<string>() { AggregationsSubQueries.SummarizableMetricsQuery };

    /// <inheritdoc/>
    public void Visit(AggregationDictionary aggregationDictionary)
    {
        Ensure.IsNotNull(aggregationDictionary, nameof(aggregationDictionary));

        var query = new StringBuilder();
        var (_, firstAggregationContainer) = aggregationDictionary.First();

        if (aggregationDictionary.Count == 1 && firstAggregationContainer.PrimaryAggregation is BucketAggregation)
        {
            // This is a bucket aggregation scenario.
            // We delegate the KQL syntax construction to the aggregation container.
            firstAggregationContainer.Accept(this);
            query.Append(firstAggregationContainer.KustoQL);
        }
        else
        {
            // This is not a bucket aggregation scenario.
            var defaultKey = Guid.NewGuid().ToString();

            var defaultAggregation = new AggregationContainer()
            {
                PrimaryAggregation = new DefaultAggregation() { Key = defaultKey },
                SubAggregations = aggregationDictionary,
            };

            defaultAggregation.Accept(this);
            query.Append(defaultAggregation.KustoQL);
        }

        aggregationDictionary.KustoQL = query.ToString();
    }
}
