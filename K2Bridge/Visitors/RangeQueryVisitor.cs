namespace K2Bridge.Visitors
{
    using System;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeQuery rangeQuery)
        {
            if (rangeQuery.Format == "epoch_millis")
            {
                // default time filter through a range query uses epoch times with GTE+LTE
                if (rangeQuery.GTEValue == null)
                {
                    throw new NullReferenceException("RangeQuery GTE value is null");
                }

                if (rangeQuery.LTEValue == null)
                {
                    throw new NullReferenceException("RangeQuery LTE value is null");
                }

                rangeQuery.KQL = $"{rangeQuery.FieldName} >= fromUnixTimeMilli({rangeQuery.GTEValue}) {KQLOperators.And} {rangeQuery.FieldName} <= fromUnixTimeMilli({rangeQuery.LTEValue})";
            }
            else
            {
                // general "is between" filter on numeric fields uses a range query with GTE+LT (not LTE like above)
                if (rangeQuery.GTEValue == null)
                {
                    throw new NullReferenceException("RangeQuery GTE value is null");
                }

                if (rangeQuery.LTValue == null)
                {
                    throw new NullReferenceException("RangeQuery LT value is null");
                }

                rangeQuery.KQL = $"{rangeQuery.FieldName} >= {rangeQuery.GTEValue} and {rangeQuery.FieldName} < {rangeQuery.LTValue}";
            }
        }
    }
}
