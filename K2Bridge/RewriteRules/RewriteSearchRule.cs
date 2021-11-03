﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.RewriteRules
{
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Normalizes the route of IndexList.
    /// </summary>
    internal class RewriteSearchRule : IRule
    {
        /// <summary>
        /// Apply this rule on the given context object, i.e. add trailing slashes
        /// if needed at the end of the request path.
        /// In this case all requests containing _search in the path are directed
        /// to the IndexListController.
        /// Requests to /_resolve/index are also sent to IndexListController
        /// (for Kibana 7).
        /// We ignore /.kibana/* routs as it's internal for Kibana.
        /// </summary>
        /// <param name="context">The context object which holds the request path.</param>
        public async void ApplyRule(RewriteContext context)
        {
            if (context.HttpContext.Request.Path.Value.Contains("_search", System.StringComparison.OrdinalIgnoreCase)
                && !context.HttpContext.Request.Path.Value.Contains(".kibana", System.StringComparison.OrdinalIgnoreCase))
            {
                // enable buffering to read the body stream multiple times.
                context.HttpContext.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.HttpContext.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 8 * 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();

                // Reset the request body stream position so the next middleware can read it
                context.HttpContext.Request.Body.Position = 0;

                // Although it doesn't make sense for a search query to have an empty body, we let it pass here and fail it later own when the body is analyzed.
                if (string.IsNullOrEmpty(body))
                {
                    // This is a regular search (documents) request
                    context.HttpContext.Request.Path = $"/Query/SingleSearch/{GetIndexNameFromPath(context.HttpContext.Request.Path)}";
                    return;
                }

                JObject jo = JObject.Parse(body);
                var aggsIndices = jo.SelectToken("aggs.indices.terms.field");

                if (aggsIndices != null)
                {
                    // This is a request for the index list
                    context.HttpContext.Request.Path = $"/IndexList/Process/{GetIndexNameFromPath(context.HttpContext.Request.Path)}";
                }
                else
                {
                    // This is a regular search (documents) request
                    context.HttpContext.Request.Path = $"/Query/SingleSearch/{GetIndexNameFromPath(context.HttpContext.Request.Path)}";
                }
            }
            else if (context.HttpContext.Request.Path.Value.Contains("_resolve/index", System.StringComparison.OrdinalIgnoreCase))
            {
                // Resolve index query from Kibana 7
                context.HttpContext.Request.Path = $"/IndexList/Resolve/{GetIndexNameFromResolvePath(context.HttpContext.Request.Path)}";
            }
        }

        private string GetIndexNameFromPath(PathString pathString)
        {
            var segments = pathString.ToString().Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            return segments[0];
        }

        private string GetIndexNameFromResolvePath(PathString pathString)
        {
            var segments = pathString.ToString().Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            return segments[2];
        }
    }
}
