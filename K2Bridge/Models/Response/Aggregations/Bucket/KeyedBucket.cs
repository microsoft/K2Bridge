// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations
{
    /// <summary>
    /// Describes keyed bucket response element.
    /// </summary>
    public class KeyedBucket : BucketBase, IKeyedBucket
    {
        /// <summary>
        /// Gets or sets the Key value.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Gets or sets the KeyAsString value.
        /// </summary>
        public string KeyAsString { get; set; }
    }
}