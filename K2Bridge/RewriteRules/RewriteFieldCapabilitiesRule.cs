// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.RewriteRules
{
    using System.IO;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;

    /// <summary>
    /// Normalizes the route of Fields caps.
    /// </summary>
    internal class RewriteFieldCapabilitiesRule : IRule
    {
        /// <summary>
        /// Apply this rule on the given context object, i.e. add trailing slashes
        /// if needed at the end of the request path.
        /// </summary>
        /// <param name="context">The context object which holds the request path.</param>
        public void ApplyRule(RewriteContext context)
        {
            if (context.HttpContext.Request.Path.Value.Contains("_field_caps"))
            {
                var segments = context.HttpContext.Request.Path.ToString().Split('/');
                context.HttpContext.Request.Path = "/FieldCapability/Process/" + segments[1];
                context.HttpContext.Request.QueryString = context.HttpContext.Request.QueryString;
                context.HttpContext.Request.Method = HttpMethods.Post;
                var mem = new MemoryStream();
                using (var writer = new StreamWriter(mem))
                {
                    writer.WriteLine();
                    mem.Seek(0, SeekOrigin.Current);
                    context.HttpContext.Request.Body = mem;
                }
            }
        }
    }
}
