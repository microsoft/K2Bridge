// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request
{
    /// <summary>
    /// A base for all query related object that includes the translated query property.
    /// </summary>
    internal abstract class KustoQLBase
    {
        /// <summary>
        /// Gets or sets the translation of the query.
        /// </summary>
        public string KustoQL { get; set; }
    }
}
