// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.RewriteRules
{
    using Microsoft.AspNetCore.Rewrite;

    /// <summary>
    /// This rewrite rule replaces the illegal path kibana requests
    /// in the form of
    /// /_template/kibana_index_template%3A.kibana?include_type_name=true
    /// the dot(.) is replaced with (:), and later is replaced back
    /// by the appropriate controller which handles it.
    /// </summary>
    internal class RewriteRequestsForTemplateRule : IRule
    {
        /// <summary>
        /// Apply this rule on the given context object.
        /// </summary>
        /// <param name="context">The context object which holds the request path.</param>
        public void ApplyRule(RewriteContext context)
        {
            // a workaround an illegal path. the app can' read a path
            // containing :. and replaces it, with a valid token
            if (context.HttpContext.Request.Path.Value.Contains("_template", System.StringComparison.OrdinalIgnoreCase)
                && context.HttpContext.Request.Path.Value.Contains(":.", System.StringComparison.OrdinalIgnoreCase))
            {
                context.HttpContext.Request.Path = context.HttpContext.Request.Path.Value.Replace(":.", "::", System.StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
