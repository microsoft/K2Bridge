namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Query query)
        {
            query.Bool.Accept(this);
            query.KQL = $"{query.Bool.KQL}";
        }
    }
}
