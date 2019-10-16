namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(SortClause sortClause)
        {
            if (sortClause.FieldName.StartsWith('_'))
            {
                // fields that start with "_" are internal to elastic and we want to disregard them
                sortClause.KQL = string.Empty;
            }
            else
            {
                sortClause.KQL = $"{sortClause.FieldName} {sortClause.Order}";
            }
        }
    }
}
