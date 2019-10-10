namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(SortClause sortClause)
        {
            sortClause.KQL = $"{sortClause.FieldName} {sortClause.Order}";
        }
    }
}
