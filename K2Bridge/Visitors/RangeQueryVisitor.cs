namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeQuery rangeQuery)
        {
            string betweenExp;

            if (rangeQuery.Format == "epoch_millis")
            {
                betweenExp = $"fromUnixTimeMilli({rangeQuery.GTEValue})..fromUnixTimeMilli({rangeQuery.LTEValue})";
            }
            else
            {
                betweenExp = $"{rangeQuery.GTEValue}..{rangeQuery.LTEValue}";
            }

            rangeQuery.KQL = $"{rangeQuery.FieldName} between ({betweenExp})";
        }
    }
}
