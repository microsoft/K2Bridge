// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface for a bucket class.
    /// </summary>
    public interface IBucket
    {
        /// <summary>
        /// Gets or sets documents count.
        /// </summary>
        int DocCount { get; set; }

        /// <summary>
        /// Gets or Sets aggegations dictionary.
        /// </summary>
        public Dictionary<string, List<double>> Aggs { get; set; }
    }
}
