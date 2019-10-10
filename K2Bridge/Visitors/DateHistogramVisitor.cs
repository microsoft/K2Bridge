namespace K2Bridge.Visitors
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Request.Aggregations.DateHistogram dateHistogram)
        {
            // todatetime is redundent but we'll keep it for now
            dateHistogram.KQL = $"{dateHistogram.Metric} by bin(todatetime({dateHistogram.FieldName}), {dateHistogram.Interval}) | order by todatetime({dateHistogram.FieldName}) asc";
        }
    }
}
