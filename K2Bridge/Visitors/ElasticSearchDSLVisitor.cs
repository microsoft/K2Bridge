namespace K2Bridge
{
    partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            elasticSearchDSL.Query.Accept(this);
            elasticSearchDSL.KQL = elasticSearchDSL.Query.KQL;
        }
    }
}
