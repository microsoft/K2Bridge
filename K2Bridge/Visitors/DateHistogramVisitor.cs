namespace K2Bridge
{
    using System.Collections.Generic;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Aggregations.DateHistogram dateHistogram)
        {
            dateHistogram.KQL = $"{dateHistogram.Metric} by bin({dateHistogram.FieldName}, {dateHistogram.Interval})";
        }
    }
}
