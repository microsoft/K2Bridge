// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes keyed bucket response element.
    /// </summary>
    public class KeyedBucket<TKey> : BucketBase
    {
        /// <summary>
        /// Gets or sets the Key value.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the KeyAsString value.
        /// </summary>
        public string KeyAsString { get; set; }
    }
}