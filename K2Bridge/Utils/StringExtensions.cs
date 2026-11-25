// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils;

using System;
using System.Globalization;
using System.Text;

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

    public static string QuoteKustoTable(this string table) => $"[{EncodeKustoStringLiteral(table, '\'')}]";

    /// <summary>
    /// Encodes a value as a complete double-quoted KQL string literal.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>The quoted and escaped string literal.</returns>
    public static string ToKustoStringLiteral(this string value) => EncodeKustoStringLiteral(value, '"');

    public static string EscapeSlashesAndQuotes(this string str)
    {
        Ensure.IsNotNullOrEmpty(str, nameof(str), "Input cannot be null or empty");

        return str.EscapeSlashes().Replace(@"""", @"\""", StringComparison.OrdinalIgnoreCase);
    }

    private static string EncodeKustoStringLiteral(string value, char quote)
    {
        Ensure.IsNotNull(value, nameof(value));

        var literal = new StringBuilder().Append(quote);
        foreach (var character in value)
        {
            if (character == '\\' || character == quote)
            {
                literal.Append('\\').Append(character);
            }
            else if (char.IsControl(character) || character is '\u2028' or '\u2029')
            {
                literal.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
            }
            else
            {
                literal.Append(character);
            }
        }

        return literal.Append(quote).ToString();
    }
}
