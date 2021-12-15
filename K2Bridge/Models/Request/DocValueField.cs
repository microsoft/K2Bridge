// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request
{
    /// <summary>
    /// Allows to return the doc value representation of a field for each hit.
    /// </summary>
    public class DocValueField
    {
        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        public string Field { get; internal set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public string Format { get; internal set; }
    }
}
