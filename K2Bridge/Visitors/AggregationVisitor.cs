namespace K2Bridge
{
    using System.Collections.Generic;
    using System.Text;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(Models.Aggregations.Aggregation aggregation)
        {
            if (aggregation.PrimaryAggregation == null)
            {
                return;
            }

            aggregation.PrimaryAggregation.Accept(this);

            // TODO: do something with the sub aggregations to KQL
            foreach (var aggKeyPair in aggregation.SubAggregations)
            {
                string subName = aggKeyPair.Key;
                var subAgg = aggKeyPair.Value;
                subAgg.Accept(this);

                aggregation.KQL += $"{subAgg.KQL}, "; // this won't work when 2+ bucket aggregations are used!
            }

            aggregation.KQL += aggregation.PrimaryAggregation.KQL;
        }
    }
}
