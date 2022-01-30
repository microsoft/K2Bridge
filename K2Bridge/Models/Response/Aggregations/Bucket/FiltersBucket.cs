// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations.Bucket;

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

/// <summary>
/// Describes terms bucket response element.
/// </summary>
[JsonConverter(typeof(FiltersBucketConverter))]
public class FiltersBucket : KeyedBucket
{
}
