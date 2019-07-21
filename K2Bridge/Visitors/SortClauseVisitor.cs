namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(SortClause sortClause)
        {
            sortClause.KQL = $"{sortClause.FieldName} {sortClause.Order}";
        }
    }
}
