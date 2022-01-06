// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System.Text.RegularExpressions;

    /// <content>
    /// Parser for the Date Math expressions.
    /// </content>
    public class DateMathParser
    {
        public static string ParseDateMath(string expr)
        {
            // See Wiki or ES docs for details on date math syntax
            var pattern = @"^(?<anchor>now|.+?(?:\|\||$))(?<ranges>(?:(?:\+|\-)[^\/]*))?(?<rounding>\/(?:y|M|w|d|h|H|m|s))?$";

            Regex rgx = new Regex(pattern);
            Match match = rgx.Match(expr);

            var kexpr = string.Empty;

            if (match.Success)
            {
                // The date literal, ie.g. YYYY-MM-DD or "now"
                if (match.Groups["anchor"].Value.Equals("now"))
                {
                    kexpr = "now()";
                }
                else
                {
                    kexpr = $"make_datetime('{match.Groups["anchor"].Value.TrimEnd('|').TrimEnd('|')}')";
                }

                // Date operation, e.g. -1M, +3d, etc.
                if (match.Groups["ranges"].Success)
                {
                    var rangeStr = match.Groups["ranges"].Value;
                    var valueStr = rangeStr.Substring(0, rangeStr.Length - 1);
                    var unitStr = rangeStr[^1];
                    var value = int.Parse(valueStr[0] == '+' ? valueStr.Substring(1) : valueStr);
                    var unit = unitStr switch
                    {
                        'y' => "year",
                        'M' => "month",
                        'w' => "week",
                        'd' => "day",
                        'h' => "hour",
                        'H' => "hour",
                        'm' => "minute",
                        's' => "second",
                        _ => null,
                    };

                    if (unit != null)
                    {
                        kexpr = $"datetime_add('{unit}', {value}, {kexpr})";
                    }
                }

                // Rounding, e.g. /d, /h, /m, etc.
                if (match.Groups["rounding"].Success)
                {
                    var unit = match.Groups["rounding"].Value.Substring(1);
                    kexpr = unit switch
                    {
                        "y" => $"startofyear({kexpr})",
                        "M" => $"startofmonth({kexpr})",
                        "w" => $"startofweek({kexpr})",
                        "d" => $"startofday({kexpr})",
                        "h" => $"bin({kexpr}, 1h)",
                        "H" => $"bin({kexpr}, 1h)",
                        "m" => $"bin({kexpr}, 1m)",
                        "s" => $"bin({kexpr}, 1s)",
                        _ => kexpr,
                    };
                }
            }

            return kexpr;
        }
    }
}