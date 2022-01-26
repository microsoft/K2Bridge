// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    using System;

    /// <summary>
    /// String extensions methods.
    /// </summary>
    public static class StringExtensions
    {
        public static string EscapeSlashes(this string str)
        {
            Ensure.IsNotNullOrEmpty(str, nameof(str), "Input cannot be null");

            return str.Replace(@"\", @"\\", StringComparison.OrdinalIgnoreCase);
        }

        public static string QuoteKustoTable(this string table) => $"['{table}']";

        public static string EscapeSlashesAndQuotes(this string str)
        {
            Ensure.IsNotNullOrEmpty(str, nameof(str), "Input cannot be null or empty");

            return str.EscapeSlashes().Replace(@"""", @"\""", StringComparison.OrdinalIgnoreCase);
        }
    }
}
