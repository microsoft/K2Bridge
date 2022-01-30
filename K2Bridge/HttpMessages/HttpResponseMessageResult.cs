// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.HttpMessages;

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
/// controllers expected type
/// The class is converting Elastic response which can't be forwarded as is to Kibana
/// with it's Http Response properties such as status code, headers etc
/// Therefore those properties are copied to IActionResult.
/// </summary>
public class HttpResponseMessageResult : IActionResult
{
    private readonly HttpResponseMessage responseMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseMessageResult"/> class.
    /// </summary>
    /// <param name="responseMessage">HttpResponseMessage returned from Elastic.</param>
    internal HttpResponseMessageResult(HttpResponseMessage responseMessage)
    {
        this.responseMessage =
            responseMessage ?? throw new ArgumentNullException(nameof(responseMessage));
    }

    /// <summary>
    /// Parses instance response and executes the result operation of the action method asynchronously.
    /// This method is called by MVC to process the result of an action method.
    /// </summary>
    /// <param name="context">The context in which the result is executed. The context information includes
    /// information about the action that was executed and request information.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    /// <exception cref="InvalidOperationException">An exception thrown when the request is invalid.</exception>
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context?.HttpContext?.Response;
        Ensure.IsNotNull(response, nameof(response), "Response message can not be null");

        using (responseMessage)
        {
            response.StatusCode = (int)responseMessage.StatusCode;

            var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
            if (responseFeature != null)
            {
                responseFeature.ReasonPhrase = responseMessage.ReasonPhrase;
            }

            var responseHeaders = responseMessage.Headers;

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

            if (responseMessage.Content != null)
            {
                var contentHeaders = responseMessage.Content.Headers;

                // Copy the response content headers only after ensuring they are complete.
                // We ask for Content-Length first because HttpContent lazily computes this
                // and only afterwards writes the value into the content headers.
                _ = contentHeaders.ContentLength;

                foreach (var header in contentHeaders)
                {
                    response.Headers.Append(header.Key, header.Value.ToArray());
                }

                await responseMessage.Content.CopyToAsync(response.Body);
            }
        }
    }
}
