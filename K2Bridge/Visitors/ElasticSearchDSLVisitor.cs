namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            var kqlSB = new StringBuilder();

            // base query
            elasticSearchDSL.Query.Accept(this);
            kqlSB.Append($"let _data = materialize({elasticSearchDSL.IndexName} | {elasticSearchDSL.Query.KQL});");

            // aggregations
            // TODO: procress the entire list
            if (elasticSearchDSL.Aggregations != null && elasticSearchDSL.Aggregations.Count > 0)
            {
                kqlSB.Append('\n').Append("(_data | summarize ");

                foreach (var aggKeyPair in elasticSearchDSL.Aggregations)
                {
                    string name = aggKeyPair.Key;
                    var aggregation = aggKeyPair.Value;
                    aggregation.Accept(this);

                    kqlSB.Append($"{aggregation.KQL} ");
                }

                kqlSB.Append("| as aggs);");
            }

            // hits (projections...)
            kqlSB.Append("\n(_data ");
            if (elasticSearchDSL.Size > 0)
            {
                // we only need to sort if we're returning hits
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    if (!string.IsNullOrEmpty(sortClause.KQL))
                    {
                        orderingList.Add(sortClause.KQL);
                    }
                }

                if (orderingList.Count > 0)
                {
                    kqlSB.Append($"| order by {string.Join(", ", orderingList)} ");
                }
            }

            if (elasticSearchDSL.Size >= 0)
            {
                kqlSB.Append($"| limit {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KQL = kqlSB.ToString();
        }
    }
}
