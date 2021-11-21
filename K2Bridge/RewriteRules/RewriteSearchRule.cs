// Copyright (c) Microsoft Corporation. All rights reserved.
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
        /// We ignore /.kibana/* routs as it's internal for Kibana.
        /// </summary>
        /// <param name="context">The context object which holds the request path.</param>
        public void ApplyRule(RewriteContext context)
        {
            if (context.HttpContext.Request.Path.Value.Contains("_search", System.StringComparison.OrdinalIgnoreCase)
                && !context.HttpContext.Request.Path.Value.Contains(".kibana", System.StringComparison.OrdinalIgnoreCase))
            {
                context.HttpContext.Request.Path = $"/Query/SingleSearch/{GetIndexNameFromPath(context.HttpContext.Request.Path)}";
            }
        }

        private string GetIndexNameFromPath(PathString pathString)
        {
            var segments = pathString.ToString().Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            return segments[0];
        }
    }
}
