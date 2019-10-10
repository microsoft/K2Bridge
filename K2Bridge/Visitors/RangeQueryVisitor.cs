namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(RangeQuery rangeQuery)
        {
            string betweenExp;

            if (rangeQuery.Format == "epoch_millis")
            {
                betweenExp = $"fromUnixTimeMilli({rangeQuery.GTEValue}) .. fromUnixTimeMilli({rangeQuery.LTEValue})";
            }
            else
            {
                betweenExp = $"{rangeQuery.GTEValue} .. {rangeQuery.LTValue}";
            }

            rangeQuery.KQL = $"{rangeQuery.FieldName} between ({betweenExp})";
        }
    }
}
