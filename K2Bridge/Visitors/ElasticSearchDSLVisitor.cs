namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            elasticSearchDSL.Query.Accept(this);

            string kql = elasticSearchDSL.Query.KQL;

            if (elasticSearchDSL.Size >= 0)
            {
                kql += $"\n| limit {elasticSearchDSL.Size}";
            }

            elasticSearchDSL.KQL = kql;
        }
    }
}
