namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(BoolClause boolClause)
        {
            string tempKQL = string.Empty;

            bool multipleLeafQueries = false;
            if (boolClause.Must.Count > 1)
            {
                multipleLeafQueries = true;
            }

            foreach (dynamic leafQuery in boolClause.Must)
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);

                if (multipleLeafQueries)
                {
                    tempKQL += $"({leafQuery.KQL}) AND ";
                }
                else
                {
                    boolClause.KQL = $"{leafQuery.KQL}";
                    return;
                }
            }

            boolClause.KQL = tempKQL.Remove(tempKQL.Length - 5);
        }
    }
}
