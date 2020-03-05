// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models
{
    using System;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A class to hold different properties that will propagate in K2 and will later be a part of the request to Kusto.
    /// </summary>
    public class RequestContext
    {
        private const string CorrelationIdHeader = "x-correlation-id";

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContext"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Context value.</param>
        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            Ensure.IsNotNull(httpContextAccessor, nameof(httpContextAccessor));

            // Header is added in CorrelationIdMiddleware
            CorrelationId = Guid.Parse(httpContextAccessor.HttpContext.Request.Headers[CorrelationIdHeader]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContext"/> class.
        /// </summary>
        public RequestContext()
        {
        }

        /// <summary>
        /// Gets or sets a Guid that is used for logs and traces.
        /// </summary>
        public Guid CorrelationId { get; set; }
    }
}
