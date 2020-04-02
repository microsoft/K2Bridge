// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Utils
{
    using System;

    /// <summary>
    /// String extensions methods
    /// </summary>
    public static class StringExtensions
    {
        public static string EscapeSlashes(this String str)
        {
            return str.Replace(@"\", @"\\", StringComparison.OrdinalIgnoreCase);
        }
    }
}