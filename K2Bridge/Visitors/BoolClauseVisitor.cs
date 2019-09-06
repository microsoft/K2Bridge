namespace K2Bridge
{
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string KQLAndKeyword = "and";
        private const string KQLNotKeyword = "not";

        public void Visit(BoolClause boolClause)
        {
            bool addedSearchToKQL = false;
            bool addedWhereToKQL = false;

            string mustKQL = string.Empty;
            foreach (dynamic leafQuery in boolClause.Must)
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);

                // if we used the search bar - the first leaf in Must will be of type QueryStringQuery
                if (leafQuery is QueryStringQuery)
                {
                    addedSearchToKQL = true;
                    mustKQL += $"{leafQuery.KQL}";
                }
                else
                {
                    if (!addedWhereToKQL)
                    {
                        if (addedSearchToKQL)
                        {
                            mustKQL += "\n| ";
                        }

                        mustKQL += "where ";
                        addedWhereToKQL = true;
                    }
                    else
                    {
                        mustKQL += $" {KQLAndKeyword} ";
                    }

                    mustKQL += $"({leafQuery.KQL})";
                }
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
                if (!addedWhereToKQL)
                {
                    if (addedSearchToKQL)
                    {
                        mustNotKQL += "\n| ";
                    }

                    mustNotKQL += "where ";
                    addedWhereToKQL = true;
                }
                else
                {
                    mustNotKQL += $" {KQLAndKeyword} ";
                }

                mustNotKQL += $"{KQLNotKeyword} ({leafQuery.KQL})";
            }

            boolClause.KQL = $"{mustKQL}{mustNotKQL}";
        }
    }
}