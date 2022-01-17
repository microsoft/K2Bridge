// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    /// <summary>
    /// Provides Kusto operators constants to be used across the visitors classes.
    /// They are listed to group them in one place and to avoid hard coded strings.
    /// We are using KustoQL instead of the regular KQL since Kibana also has
    /// its own KQL term...
    /// </summary>
    public static class KustoQLOperators
    {
#pragma warning disable SA1600 // Elements should be documented
        public const string Count = "count";
        public const string And = "and";
        public const string Or = "or";
        public const string Not = "not";
        public const string Contains = "contains";
        public const string Has = "has";
        public const string Where = "where";
        public const string OrderBy = "order by";
        public const string Let = "let";
        public const string Limit = "limit";
        public const string Pack = "pack";
        public const string Summarize = "summarize";
        public const string Materialize = "materialize";
        public const string DCount = "dcount";
        public const string StDev = "stdev";
        public const string StDevPopulation = "stdevp";
        public const string Avg = "avg";
        public const string Min = "min";
        public const string Max = "max";
        public const string Sum = "sum";
        public const string Pow = "pow";
        public const string Variance = "variance";
        public const string VariancePopulation = "variancep";
        public const string PercentilesArray = "percentiles_array";
        public const string IsNotNull = "isnotnull";
        public const string StartOfWeek = "startofweek";
        public const string StartOfMonth = "startofmonth";
        public const string StartOfYear = "startofyear";
        public const string ToDateTime = "todatetime";
        public const string Database = "database";
        public const string Databases = "databases";
        public const string Functions = "functions";
        public const string Schema = "schema";
        public const string Project = "project";
        public const string Search = "search";
        public const string Distinct = "distinct";
        public const string HasPrefix = "hasprefix";
        public const string MatchRegex = "matches regex";
        public const string GetSchema = "getschema";
        public const string BuildSchema = "buildschema";
        public const string ToScalar = "toscalar";
        public const string ToInt = "toint";
        public const string ToDouble = "todouble";

        // Renamed to avoid conflict with ToString()
        public const string ToStringOperator = "tostring";
        public const string Floor = "floor";
        public const string Sample = "sample";
        public const string Equal = "==";
        public const string Case = "case";
        public const string Extend = "extend";
        public const string ProjectReorder = "project-reorder";
        public const string ProjectRename = "project-rename";
        public const string Union = "union";
        public const string PackArray = "pack_array";
        public const string MvExpand = "mv-expand";
        public const string CommandSeparator = "\n| ";
#pragma warning restore SA1600 // Elements should be documented
    }
}
