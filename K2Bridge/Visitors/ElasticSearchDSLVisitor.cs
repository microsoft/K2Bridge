namespace K2Bridge
{
    using System.Collections.Generic;
    using System.Text;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            var kqlSB = new StringBuilder();

            // REMOVE THIS
            kqlSB.Append("let fromUnixTimeMilli = (t:long) { datetime(1970 - 01 - 01) + t * 1millisec};").Append('\n');

            // base query
            elasticSearchDSL.Query.Accept(this);
            kqlSB.Append($"let _data = ({elasticSearchDSL.IndexName} | evaluate bag_unpack(raw) | evaluate bag_unpack(_source)\n| {elasticSearchDSL.Query.KQL});");

            // aggregations
            // TODO: procress the entire list
            if (elasticSearchDSL.Aggregations.Count > 0)
            {
                kqlSB.Append('\n').Append("(_data | summarize ");

                foreach (var aggKeyPair in elasticSearchDSL.Aggregations)
                {
                    string name = aggKeyPair.Key;
                    var aggregation = aggKeyPair.Value;
                    aggregation.Accept(this);

                    kqlSB.Append($"{aggregation.KQL} ");
                }

                kqlSB.Append(" | as aggs);");
            }

            // hits (projections...)
            kqlSB.Append("\n(_data ");
            if (elasticSearchDSL.Size > 0)
            {
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    orderingList.Add(sortClause.KQL);
                }

                kqlSB.Append($"| order by {string.Join(", ", orderingList)}");
            }

            if (elasticSearchDSL.Size >= 0)
            {
                kqlSB.Append($"| limit {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KQL = kqlSB.ToString();
        }
    }
}
