namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Queries;
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ExistsQuery existsQuery)
        {
            existsQuery.KQL = $"isnotnull({existsQuery.FieldName})";
        }
    }
}
