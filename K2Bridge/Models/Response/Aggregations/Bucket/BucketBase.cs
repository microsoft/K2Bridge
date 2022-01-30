// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response.Aggregations.Bucket
{
    /// <summary>
    /// Describes abstract base class for bucket response element.
    /// </summary>
    public abstract class BucketBase : AggregateDictionary
    {
        /// <summary>
        /// Gets or sets the DocCount value.
        /// </summary>
        public virtual long DocCount { get; set; }
    }
}
