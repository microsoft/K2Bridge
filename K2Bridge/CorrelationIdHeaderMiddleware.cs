// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Middleware to set CorrelationId headers in HttpContext's Request and Response.
    /// </summary>
    public class CorrelationIdHeaderMiddleware
    {
        private const string CorrelationIdHeader = "x-correlation-id";
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdHeaderMiddleware"/> class.
        /// A middleware to handle correlation id.
        /// </summary>
        /// <param name="next">The RequestDelegate.</param>
        public CorrelationIdHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Gets CorrelationId header from Request (or sets a new one), and sets it in Response.
        /// </summary>
        /// <param name="httpContext">The HttpContext of the request.</param>
        /// <returns>A <see cref="Task"/> representing the work in the middleware.</returns>
        public Task Invoke(HttpContext httpContext)
        {
            var correlationId = GetCorrelationIdHeaderOrGenerateNew(httpContext.Request.Headers);

            httpContext.Response.Headers[CorrelationIdHeader] = correlationId.ToString();

            return next(httpContext);
        }

        /// <summary>
        /// Extract headerName's value from dictionary or return <see cref="defaultValue"/>.
        /// </summary>
        /// <param name="dic"><see cref="IHeaderDictionary"/> to extract the headerName from.</param>
        /// <param name="headerName">Header's name.</param>
        /// <param name="defaultValue">Default value in case headerName does not exist.</param>
        /// <returns>The string which is the value of <see cref="headerName"/> in <see cref="dic"/>.</returns>
        private static string GetHeaderOrDefault(IHeaderDictionary dic, string headerName, string defaultValue = null)
        {
            Ensure.IsNotNull(dic, nameof(HeaderDictionary));
            Ensure.IsNotNullOrEmpty(headerName, nameof(headerName));

            return dic.TryGetValue(headerName, out var value) && value.Any()
                ? value.First()
                : defaultValue;
        }

        /// <summary>
        /// Extract value of header 'x-correlation-id' from dictionary or add a newly generated value to dictionary and return it.
        /// </summary>
        /// <param name="dic"><see cref="IHeaderDictionary"/> to extract the value from.</param>
        /// <returns>The string which is the value of x-correlation-id headerName in <see cref="dic"/>.</returns>
        private static Guid GetCorrelationIdHeaderOrGenerateNew(IHeaderDictionary dic)
        {
            Ensure.IsNotNull(dic, nameof(HeaderDictionary));

            var correlationId = GetHeaderOrDefault(dic, CorrelationIdHeader);

            if (!Guid.TryParse(correlationId, out var guid))
            {
                guid = Guid.NewGuid();
                dic[CorrelationIdHeader] = guid.ToString();
            }

            return guid;
        }
    }
}