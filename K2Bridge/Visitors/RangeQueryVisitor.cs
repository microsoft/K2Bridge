// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Queries.RangeClause rangeClause)
        {
            if (string.IsNullOrEmpty(rangeClause.FieldName))
            {
                throw new IllegalClauseException("RangeClause clause must have a valid FieldName property");
            }

            if (rangeClause.Format == "epoch_millis")
            {
                // default time filter through a rangeClause query uses epoch times with GTE+LTE
                if (rangeClause.GTEValue == null)
                {
                    throw new IllegalClauseException("RangeClause clause must have a valid GTEValue property");
                }

                if (rangeClause.LTEValue == null)
                {
                    throw new IllegalClauseException("RangeClause clause must have a valid LTEValue property");
                }

                rangeClause.KQL = $"{rangeClause.FieldName} >= fromUnixTimeMilli({rangeClause.GTEValue}) {KQLOperators.And} {rangeClause.FieldName} <= fromUnixTimeMilli({rangeClause.LTEValue})";
            }
            else
            {
                // general "is between" filter on numeric fields uses a rangeClause query with GTE+LT (not LTE like above)
                if (rangeClause.GTEValue == null)
                {
                    throw new IllegalClauseException("RangeClause clause must have a valid GTEValue property");
                }

                if (rangeClause.LTValue == null)
                {
                    throw new IllegalClauseException("RangeClause clause must have a valid LTValue property");
                }

                rangeClause.KQL = $"{rangeClause.FieldName} >= {rangeClause.GTEValue} and {rangeClause.FieldName} < {rangeClause.LTValue}";
            }
        }
    }
}
