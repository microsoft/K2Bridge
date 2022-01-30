// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations;

using System.Collections.Generic;

/// <summary>
/// Describes aggregate dictionary response element.
/// This dictionary contains different type of aggregates family:
/// - <see cref="ValueAggregate"/> contains metric aggregation value.
/// - <see cref="PercentileAggregate"/> contains percentile aggregation values.
/// - <see cref="BucketAggregate"/> describes bucket aggregation.
/// </summary>
public class AggregateDictionary : Dictionary<string, IAggregate>
{
}
