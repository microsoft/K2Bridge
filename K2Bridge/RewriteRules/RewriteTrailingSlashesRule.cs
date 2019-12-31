// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.RewriteRules
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;

    /// <summary>
    /// This rewrite rule ensure that HTTP request path has a valid form.
    /// Check if the request path is a valid file (name.extension). If it is not
    /// then this rule ensures that the path has a trailing slash.
    /// There can be path segements have the form of /.pathsegment (sent by Kibana).
    /// ASP.NET interprets this as a file but not a path.
    /// </summary>
    internal class RewriteTrailingSlashesRule : IRule
    {
        /// <summary>
        /// Apply this rule on the given context object, i.e. add trailing slashes
        /// if needed at the end of the request path.
        /// </summary>
        /// <param name="context">The context object which holds the request path.</param>
        public void ApplyRule(RewriteContext context)
        {
            context.HttpContext.Request.Path =
                this.RewritePath(context.HttpContext.Request.Path);
        }

        /// <summary>
        /// Add trailing slashes if needed at the end of the request path.
        /// </summary>
        /// <param name="requestPath">A request path for examination.</param>
        /// <returns>The same path with trailing slash (if needed).</returns>
        internal PathString RewritePath(PathString requestPath)
        {
            PathString result = requestPath;

            var segments = requestPath.ToString().Split('/');
            var lastSegment = segments[^1];
            if (lastSegment != string.Empty)
            {
                // no trailing slash at the end
                // check if the last segment is a valid filename
                var fileSegemnts = lastSegment.Split('.');
                if (fileSegemnts.Length != 2 || fileSegemnts.Any(seg => string.IsNullOrEmpty(seg)))
                {
                    result += '/';
                }
            }

            return result;
        }
    }
}
