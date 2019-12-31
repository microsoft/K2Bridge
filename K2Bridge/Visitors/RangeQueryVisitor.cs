// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Queries.Range range)
        {
            if (string.IsNullOrEmpty(range.FieldName))
            {
                throw new IllegalClauseException("Range clause must have a valid FieldName property");
            }

            if (range.Format == "epoch_millis")
            {
                // default time filter through a range query uses epoch times with GTE+LTE
                if (range.GTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid GTEValue property");
                }

                if (range.LTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid LTEValue property");
                }

                range.KQL = $"{range.FieldName} >= fromUnixTimeMilli({range.GTEValue}) {KQLOperators.And} {range.FieldName} <= fromUnixTimeMilli({range.LTEValue})";
            }
            else
            {
                // general "is between" filter on numeric fields uses a range query with GTE+LT (not LTE like above)
                if (range.GTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid GTEValue property");
                }

                if (range.LTValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid LTValue property");
                }

                range.KQL = $"{range.FieldName} >= {range.GTEValue} and {range.FieldName} < {range.LTValue}";
            }
        }
    }
}
