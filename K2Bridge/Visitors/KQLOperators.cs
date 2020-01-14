// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    /// <summary>
    /// Provides some constants to be used across the visitors classes.
    /// </summary>
    public static class KQLOperators
    {
        public const string And = "and";
        public const string Or = "or";
        public const string Not = "not";
        public const string Contains = "contains";
        public const string Where = "where";
        public const string OrderBy = "order by";
        public const string Let = "let";
        public const string Limit = "limit";
        public const string Summarize = "summarize";
        public const string Materialize = "materialize";
        public const string DCount = "dcount";
        public const string Avg = "avg";
        public const string IsNotNull = "isnotnull";
        public const string StartOfWeek = "startofweek";
        public const string StartOfMonth = "startofmonth";
        public const string StartOfYear = "startofyear";
        public const string ToDateTime = "todatetime";
        public const string Database = "database";
        public const string Databases = "databases";
        public const string Schema = "schema";
        public const string Project = "project";
        public const string Search = "search";
        public const string Distinct = "distinct";
        public const string CommandSeparator = "\n| ";
    }
}
