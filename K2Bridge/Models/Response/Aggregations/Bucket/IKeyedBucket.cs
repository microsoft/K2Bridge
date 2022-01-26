// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations.Bucket
{
    /// <summary>
    /// Interface for keyed bucket response element.
    /// </summary>
    public interface IKeyedBucket
    {
        /// <summary>
        /// Gets or sets the Key value.
        /// </summary>
        object Key { get; set; }

        /// <summary>
        /// Gets or sets the KeyAsString value.
        /// </summary>
        string KeyAsString { get; set; }
    }
}
