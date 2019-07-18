namespace K2Bridge
{
    partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeQuery rangeQuery)
        {
            rangeQuery.KQL = $"{rangeQuery.FieldName} between ({rangeQuery.GTEValue}..{rangeQuery.LTEValue})";
        }
    }
}
