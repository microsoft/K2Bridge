namespace K2Bridge
{
    using System.Collections.Generic;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            elasticSearchDSL.Query.Accept(this);

            string kql = elasticSearchDSL.Query.KQL;

            if (elasticSearchDSL.Size > 0)
            {
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    orderingList.Add(sortClause.KQL);
                }

                kql += $"\n| order by {string.Join(", ", orderingList)}";
            }

            if (elasticSearchDSL.Size >= 0)
            {
                kql += $"\n| limit {elasticSearchDSL.Size}";
            }

            elasticSearchDSL.KQL = kql;
        }
    }
}
