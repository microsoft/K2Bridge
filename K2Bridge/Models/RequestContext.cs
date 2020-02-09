// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using System;

    /// <summary>
    /// A class to hold different properties that will propagate in K2 and will later be a part of the request to Kusto.
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// Gets or sets a Guid that is used for logs and traces.
        /// </summary>
        public Guid CorrelationId { get; set; }
    }
}
