// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Controllers
{
    using System;
    using System.Linq;

    /// <summary>
    /// Static Methods used by controllers to extract data.
    /// </summary>
    internal static class ControllerExtractMethods
    {
        private const string TemplateQueryStringSeparator = ":.";
        private const string AltTemplateQueryStringSeparator = "::";
        private const string TemplateString = @"/_template/";

        /// <summary>
        /// Partitions a NDJson query body by new line characther and
        /// returns the first and second elements if exist as tuple.
        /// </summary>
        /// <param name="queryBody">query body.</param>
        /// <returns>Tuple of first and second elements.</returns>
        internal static (string, string) SplitQueryBody(string queryBody)
        {
            var splitString = string.IsNullOrEmpty(queryBody) ? Enumerable.Empty<string>() : queryBody.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
            return (splitString.ElementAtOrDefault(0), splitString.ElementAtOrDefault(1));
        }

        /// <summary>
        /// Replaces occourances of :. with :: for strings containing _template substring
        /// a workaround an illegal path. the app can' read a path
        /// containing :. and replaces it, with a valid token.
        /// </summary>
        /// <param name="templateString">string to replace tokens in.</param>
        /// <returns>string with illegal charachter replaced.</returns>
        internal static string ReplaceTemplateString(string templateString) =>
            !string.IsNullOrEmpty(templateString) && templateString.Contains(TemplateString, StringComparison.OrdinalIgnoreCase) ?
                templateString.Replace(TemplateQueryStringSeparator, AltTemplateQueryStringSeparator, StringComparison.OrdinalIgnoreCase) :
                templateString;

        /// <summary>
        /// Replaces back occourances of :: with :. for strings containing _template substring
        /// a workaround an illegal path. the app can' read a path
        /// containing :. and replaces it, with a valid token.
        /// </summary>
        /// <param name="templateString">string to replace tokens in.</param>
        /// <returns>string with character replaced back.</returns>
        internal static string ReplaceBackTemplateString(string templateString) =>
            !string.IsNullOrEmpty(templateString) && templateString.Contains(TemplateString, StringComparison.OrdinalIgnoreCase) ?
                templateString.Replace(AltTemplateQueryStringSeparator, TemplateQueryStringSeparator, StringComparison.OrdinalIgnoreCase) :
                templateString;
    }
}
