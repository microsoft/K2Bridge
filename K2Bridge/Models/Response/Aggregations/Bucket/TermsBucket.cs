// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.JsonConverters;
using Newtonsoft.Json;

namespace K2Bridge.Models.Response.Aggregations.Bucket;

/// <summary>
/// Describes terms bucket response element.
/// </summary>
[JsonConverter(typeof(TermsBucketConverter))]
public class TermsBucket : KeyedBucket
{
}
