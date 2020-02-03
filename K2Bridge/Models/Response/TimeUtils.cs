// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Response
{
    using System;

    public static class TimeUtils
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToEpochMilliseconds(DateTime value)
        {
            var epochTime = value.Subtract(Epoch).TotalMilliseconds;
            return Convert.ToInt64(epochTime);
        }
    }
}
