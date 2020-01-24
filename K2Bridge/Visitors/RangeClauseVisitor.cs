// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeClause rangeClause)
        {
            Ensure.IsNotNull(rangeClause, nameof(rangeClause));
            EnsureClause.StringIsNotNullOrEmpty(rangeClause.FieldName, nameof(rangeClause.FieldName));
            EnsureClause.IsNotNull(rangeClause.GTEValue, nameof(rangeClause.GTEValue));

            if (rangeClause.Format == "epoch_millis")
            {
                // default time filter through a rangeClause query uses epoch times with GTE+LTE
                EnsureClause.IsNotNull(rangeClause.LTEValue, nameof(rangeClause.LTEValue));

                rangeClause.KQL = $"{rangeClause.FieldName} >= fromUnixTimeMilli({rangeClause.GTEValue}) {KQLOperators.And} {rangeClause.FieldName} <= fromUnixTimeMilli({rangeClause.LTEValue})";
            }
            else
            {
                // general "is between" filter on numeric fields uses a rangeClause query with GTE+LT (not LTE like above)
                EnsureClause.IsNotNull(rangeClause.LTValue, nameof(rangeClause.LTValue));

                rangeClause.KQL = $"{rangeClause.FieldName} >= {rangeClause.GTEValue} and {rangeClause.FieldName} < {rangeClause.LTValue}";
            }
        }
    }
}
