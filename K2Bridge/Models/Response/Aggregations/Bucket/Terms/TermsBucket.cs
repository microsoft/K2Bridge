// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;
    using K2Bridge.JsonConverters;

    /// <summary>
    /// Describes terms bucket response element.
    /// </summary>
    [JsonConverter(typeof(TermsBucketConverter))]
    public class TermsBucket : KeyedBucket<string>
    {
    }
}