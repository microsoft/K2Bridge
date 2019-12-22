// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.HttpMessages
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// This class acts as a wrapper of the original HttpResponseMessage
    /// result object with the IActionResult implementation to meet the
    /// controllers expected type.
    /// </summary>
    public class HttpResponseMessageResult : IActionResult
    {
        // UserStory 1373:
        // https://dev.azure.com/csedevil/K2-bridge-internal/_backlogs/backlog/K2-bridge-internal%20Team/Stories/?showParents=true&workitem=1373
        private readonly HttpResponseMessage responseMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMessageResult"/> class.
        /// </summary>
        /// <param name="responseMessage"></param>
        internal HttpResponseMessageResult(HttpResponseMessage responseMessage)
        {
            this.responseMessage =
                responseMessage ?? throw new ArgumentNullException(nameof(responseMessage));
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            if (this.responseMessage == null)
            {
                var message = "Response message cannot be null";
                throw new InvalidOperationException(message);
            }

            using (this.responseMessage)
            {
                response.StatusCode = (int)this.responseMessage.StatusCode;

                var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
                if (responseFeature != null)
                {
                    responseFeature.ReasonPhrase = this.responseMessage.ReasonPhrase;
                }

                var responseHeaders = this.responseMessage.Headers;

                // Ignore the Transfer-Encoding header if it is just "chunked".
                // We let the host decide about whether the response should be chunked or not.
                if (responseHeaders.TransferEncodingChunked == true &&
                    responseHeaders.TransferEncoding.Count == 1)
                {
                    responseHeaders.TransferEncoding.Clear();
                }

                foreach (var header in responseHeaders)
                {
                    response.Headers.Append(header.Key, header.Value.ToArray());
                }

                if (this.responseMessage.Content != null)
                {
                    var contentHeaders = this.responseMessage.Content.Headers;

                    // Copy the response content headers only after ensuring they are complete.
                    // We ask for Content-Length first because HttpContent lazily computes this
                    // and only afterwards writes the value into the content headers.
                    var unused = contentHeaders.ContentLength;

                    foreach (var header in contentHeaders)
                    {
                        response.Headers.Append(header.Key, header.Value.ToArray());
                    }

                    await this.responseMessage.Content.CopyToAsync(response.Body);
                }
            }
        }
    }
}
