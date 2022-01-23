// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parser for the Date Math expressions.
    /// </summary>
    public class DateMathParser
    {
        /// <summary>
        /// Converts a date expression from Kibana syntax to Kusto/ADX.
        /// </summary>
        /// <param name="expr">A Kibana date expression to process.</param>
        /// <returns>A KustoQL string with the processed date expression.</returns>
        /// <exception cref="IllegalClauseException">In case of unknown date/time expressions.</exception>
        public static string ParseDateMath(string expr)
        {
            // See Wiki or ES docs for details on date math syntax
            var rgx = new Regex(
                @"^
                        (?: (?<anchor>now) | (?: (?<anchor>.+?) (?:\|\||$)) )
                        (?: (?<rangeSign>[+-]) (?<rangeValue>\d+) (?<rangeUnit>[a-zA-Z]))*
                        (?: (?:\/ (?<rounding>y|M|w|d|h|H|m|s)))?",
                RegexOptions.IgnorePatternWhitespace);
            var match = rgx.Match(expr);

            var kexpr = string.Empty;

            if (!match.Success)
            {
                return kexpr;
            }

            // The date literal, ie.g. YYYY-MM-DD or "now"
            var anchor = match.Groups["anchor"].Value;
            kexpr = anchor.Equals("now", System.StringComparison.Ordinal) ? "now()" : $"make_datetime('{anchor}')";

            // Date operation, e.g. -1M, +3d, etc.
            for (var i = 0; i < match.Groups["rangeSign"].Captures.Count; i++)
            {
                var sign = match.Groups["rangeSign"].Captures[i].Value == "-" ? "-" : string.Empty;
                var value = int.Parse(match.Groups["rangeValue"].Captures[i].Value);
                var unit = match.Groups["rangeUnit"].Captures[i].Value[0] switch
                {
                    'y' => "year",
                    'M' => "month",
                    'w' => "week",
                    'd' => "day",
                    'h' => "hour",
                    'H' => "hour",
                    'm' => "minute",
                    's' => "second",
                    _ => throw new IllegalClauseException("Invalid date math range unit."),
                };

                kexpr = $"datetime_add('{unit}', {sign}{value}, {kexpr})";
            }

            // Rounding, e.g. /d, /h, /m, etc.
            if (match.Groups["rounding"].Success)
            {
                kexpr = match.Groups["rounding"].Value switch
                {
                    "y" => $"startofyear({kexpr})",
                    "M" => $"startofmonth({kexpr})",
                    "w" => $"startofweek({kexpr})",
                    "d" => $"startofday({kexpr})",
                    "h" => $"bin({kexpr}, 1h)",
                    "H" => $"bin({kexpr}, 1h)",
                    "m" => $"bin({kexpr}, 1m)",
                    "s" => $"bin({kexpr}, 1s)",
                    _ => throw new IllegalClauseException("Invalid date math rounding."),
                };
            }

            return kexpr;
        }
    }
}
