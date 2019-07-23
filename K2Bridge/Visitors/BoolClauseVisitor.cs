namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string KQLAndKeyword = "and";
        private const string KQLNotKeyword = "not";

        public void Visit(BoolClause boolClause)
        {
            string mustKQL = string.Empty;
            foreach (dynamic leafQuery in boolClause.Must)
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);
                mustKQL += $"({leafQuery.KQL}) {KQLAndKeyword} ";
            }

            string mustNotKQL = string.Empty;
            foreach (dynamic leafQuery in boolClause.MustNot)
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);
                mustNotKQL += $"({KQLNotKeyword} {leafQuery.KQL}) {KQLAndKeyword} ";
            }

            boolClause.KQL = $"{mustKQL.Remove(mustKQL.Length - 5)}";
            if (!string.IsNullOrEmpty(mustNotKQL))
            {
                if (!string.IsNullOrEmpty(boolClause.KQL))
                {
                    boolClause.KQL += $" {KQLAndKeyword} ";
                }

                boolClause.KQL += $"{mustNotKQL.Remove(mustNotKQL.Length - 5)}";
            }
        }
    }
}
