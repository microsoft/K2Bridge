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
            var pattern = @"^([0-9-T:]+|now)\|*([0-9-+]*)([a-zA-Z]*)\/?([a-zA-Z]*)";

            Regex rgx = new Regex(pattern);
            Match match = rgx.Match(expr);

            var kexpr = string.Empty;

            if (match.Success)
            {
                // The date literal, ie.g. YYYY-MM-DD or "now"
                if (match.Groups[1].Value.Equals("now"))
                {
                    kexpr = "now()";
                }
                else
                {
                    kexpr = $"make_datetime('{match.Groups[1]}')";
                }

                // Date operation, e.g. -1M, +3d, etc.
                if (!string.IsNullOrEmpty(match.Groups[2].Value) && !string.IsNullOrEmpty(match.Groups[3].Value))
                {
                    var valueStr = match.Groups[2].Value;
                    var value = int.Parse(valueStr[0] == '+' ? valueStr.Substring(1) : valueStr);
                    var unit = match.Groups[3].Value switch
                    {
                        "y" => "year",
                        "M" => "month",
                        "w" => "week",
                        "d" => "day",
                        "h" => "hour",
                        "H" => "hour",
                        "m" => "minute",
                        "s" => "second",
                        _ => null,
                    };

                    if (unit != null)
                    {
                        kexpr = $"datetime_add('{unit}', {value}, {kexpr})";
                    }
                }

                // Rounding, e.g. /d, /h, /m, etc.
                if (!string.IsNullOrEmpty(match.Groups[4].Value))
                {
                    var unit = match.Groups[4].Value;
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