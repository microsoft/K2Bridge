// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;

namespace K2Bridge.Utils;

/// <summary>
/// Time utils.
/// </summary>
public static class TimeUtils
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// To Epoch Milliseconds.
    /// </summary>
    /// <param name="value">Time.</param>
    /// <returns>Epoc time.</returns>
    public static long ToEpochMilliseconds(DateTime value)
    {
        var epochTime = value.Subtract(Epoch).TotalMilliseconds;
        return Convert.ToInt64(epochTime);
    }
}
