// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    /// <content>
    /// A visitor for the <see cref="RangeClause"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(RangeClause rangeClause)
        {
            Ensure.IsNotNull(rangeClause, nameof(rangeClause));
            EnsureClause.StringIsNotNullOrEmpty(rangeClause.FieldName, nameof(rangeClause.FieldName));

            switch (rangeClause.Format)
            {
                // format used by Kibana 6
                case "epoch_millis":
                    FillKqlQuery(rangeClause, s => $"unixtime_milliseconds_todatetime({s})");
                    break;

                // format used by Kibana 7
                case "strict_date_optional_time":
                    FillKqlQuery(rangeClause, s => $"{KustoQLOperators.ToDateTime}(\"{DateTime.Parse(s).ToUniversalTime():o}\")");
                    break;

                default:
                    // general "is between" filter on numeric fields uses a rangeClause query with GTE+LT (not LTE like above)
                    var t = ClauseFieldTypeProcessor.GetType(schemaRetriever, rangeClause.FieldName).Result;
                    switch (t)
                    {
                        case ClauseFieldType.Numeric:
                            FillKqlQuery(rangeClause);
                            break;
                        case ClauseFieldType.Date:
                            FillKqlQuery(rangeClause, s => $"{KustoQLOperators.ToDateTime}(\"{DateTime.Parse(s):o}\")");
                            break;
                        case ClauseFieldType.Text:
                            throw new NotSupportedException("Text Range is not supported.");
                        case ClauseFieldType.Unknown:
                            throw new Exception($"Field name {rangeClause.FieldName} has an unknown type.");
                        default:
                            throw new IllegalClauseException();
                    }

                    break;
            }
        }

        private static void FillKqlQuery(RangeClause rangeClause, Func<string, string> valueConverter = null)
        {
            valueConverter ??= s => s;
            var (gtOperator, gtValue) = rangeClause switch
            {
                { GTValue: null, GTEValue: null } => throw new IllegalClauseException(),
                { GTValue: "*" } or { GTEValue: "*" } => (null, null),
                { GTValue: { } gt } => (">", valueConverter(gt)),
                { GTEValue: { } gte } => (">=", valueConverter(gte)),
                _ => throw new Exception("Invalid range clause."),
            };

            var (ltOperator, ltValue) = rangeClause switch
            {
                { LTValue: null, LTEValue: null } => throw new IllegalClauseException(),
                { LTValue: "*" } or { LTEValue: "*" } => (null, null),
                { LTValue: { } lt } => ("<", valueConverter(lt)),
                { LTEValue: { } lte } => ("<=", valueConverter(lte)),
                _ => throw new Exception("Invalid range clause."),
            };

            var gtQuery = gtOperator != null ? $"['{rangeClause.FieldName}'] {gtOperator} {gtValue}" : null;
            var ltQuery = ltOperator != null ? $"['{rangeClause.FieldName}'] {ltOperator} {ltValue}" : null;
            rangeClause.KustoQL = (gtQuery, ltQuery) switch
            {
                (null, null) => string.Empty,
                (null, _) => ltQuery,
                (_, null) => gtQuery,
                (_, _) => $"{gtQuery} {KustoQLOperators.And} {ltQuery}",
            };
        }
    }
}