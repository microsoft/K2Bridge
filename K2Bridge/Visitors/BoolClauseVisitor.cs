using System;

namespace K2Bridge
{
    using System.Collections.Generic;
    using System.Linq;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private const string KQLAndKeyword = "and";
        private const string KQLNotKeyword = "not";
        private const string KQLCommandSeparator = "\n| ";

        public void Visit(BoolClause boolClause)
        {
            // TODO: the following can be more generic (need to account for Should/Or)
            List<string> mustKQLExpressions = new List<string>();

            foreach (dynamic leafQuery in boolClause.Must.Where(q => !(q is QueryStringQuery)))
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);
                mustKQLExpressions.Add($"({leafQuery.KQL})");
            }

            List<string> mustNotKQLExpressions = new List<string>();

            foreach (dynamic leafQuery in boolClause.MustNot)
            {
                if (leafQuery == null)
                {
                    // probably deser. problem
                    continue;
                }

                leafQuery.Accept(this);
                mustNotKQLExpressions.Add($"{KQLNotKeyword} ({leafQuery.KQL})");
            }

            this.KQLListToString(mustKQLExpressions, KQLAndKeyword, boolClause);
            this.KQLListToString(mustNotKQLExpressions, KQLAndKeyword, boolClause);

            QueryStringQuery queryString = (QueryStringQuery)boolClause.Must.Where(q => q is QueryStringQuery).SingleOrDefault();

            if (queryString != null)
            {
                queryString.Accept(this);
                if (!string.IsNullOrEmpty(boolClause.KQL))
                {
                    boolClause.KQL += KQLCommandSeparator;
                }

                boolClause.KQL += queryString.KQL;
            }
        }

        private void KQLListToString(List<string> kqlList, string joinString, BoolClause boolClause)
        {
            joinString = $" {joinString} ";

            if (kqlList.Count > 0)
            {
                if (!string.IsNullOrEmpty(boolClause.KQL))
                {
                    boolClause.KQL += KQLCommandSeparator;
                }

                boolClause.KQL += $"where {string.Join(joinString, kqlList)}";
            }
        }
    }
}