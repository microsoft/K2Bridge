namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(BoolClause boolClause)
        {
            AddListInternal(boolClause.Must, KQLOperators.And, false /* positive */, boolClause);
            AddListInternal(boolClause.MustNot, KQLOperators.And, true /* negative */, boolClause);
            AddListInternal(boolClause.Should, KQLOperators.Or, false /* positive */, boolClause);
            AddListInternal(boolClause.ShouldNot, KQLOperators.Or, true /* negative */, boolClause);
        }

        /// <summary>
        /// Create a list of clauses of a specific type (must / must not / should / should not)
        /// </summary>
        private void AddListInternal(IEnumerable<IQueryClause> lst, string delimiterKeyword, bool negativeCondition, BoolClause boolClause)
        {
            if (lst == null) return;

            var kqlExpressions = new List<string>();

            foreach (dynamic leafQuery in lst)
            {
                if (leafQuery == null)
                {
                    // probably deserialization problem
                    continue;
                }

                leafQuery.Accept(this);
                if (negativeCondition)
                {
                    kqlExpressions.Add($"{KQLOperators.Not} ({leafQuery.KQL})");
                }
                else
                {
                    kqlExpressions.Add($"({leafQuery.KQL})");
                }
            }

            this.KQLListToString(kqlExpressions, delimiterKeyword, boolClause);
        }

        /// <summary>
        /// Joins the list of kql commands and modifies the given BoolClause
        /// </summary>
        private void KQLListToString(List<string> kqlList, string joinString, BoolClause boolClause)
        {
            joinString = $" {joinString} ";

            if (kqlList.Count > 0)
            {
                if (!string.IsNullOrEmpty(boolClause.KQL))
                {
                    boolClause.KQL += KQLOperators.CommandSeparator;
                    boolClause.KQL += KQLOperators.Where;
                    boolClause.KQL += " ";
                }

                boolClause.KQL += $"{string.Join(joinString, kqlList)}";
            }
        }
    }
}