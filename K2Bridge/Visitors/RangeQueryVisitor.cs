// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeQuery rangeQuery)
        {
            if (string.IsNullOrEmpty(rangeQuery.FieldName))
            {
                throw new IllegalClauseException("Range clause must have a valid FieldName property");
            }

            if (rangeQuery.Format == "epoch_millis")
            {
                // default time filter through a range query uses epoch times with GTE+LTE
                if (rangeQuery.GTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid GTEValue property");
                }

                if (rangeQuery.LTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid LTEValue property");
                }

                rangeQuery.KQL = $"{rangeQuery.FieldName} >= fromUnixTimeMilli({rangeQuery.GTEValue}) {KQLOperators.And} {rangeQuery.FieldName} <= fromUnixTimeMilli({rangeQuery.LTEValue})";
            }
            else
            {
                // general "is between" filter on numeric fields uses a range query with GTE+LT (not LTE like above)
                if (rangeQuery.GTEValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid GTEValue property");
                }

                if (rangeQuery.LTValue == null)
                {
                    throw new IllegalClauseException("Range clause must have a valid LTValue property");
                }

                rangeQuery.KQL = $"{rangeQuery.FieldName} >= {rangeQuery.GTEValue} and {rangeQuery.FieldName} < {rangeQuery.LTValue}";
            }
        }
    }
}
