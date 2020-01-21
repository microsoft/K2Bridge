// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using System;
    using System.Collections.Generic;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <summary>
    /// Defines a visit method for lucene boolean query.
    /// </summary>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        public void Visit(LuceneBoolQuery boolQueryWrapper)
        {
            VerifyValid(boolQueryWrapper);

            var boolClause = new BoolQuery();
            var must = new List<IQuery>();
            var should = new List<IQuery>();
            var mustNot = new List<IQuery>();
            var clauses = ((BooleanQuery)boolQueryWrapper.LuceneQuery).GetClauses();

            foreach (BooleanClause clause in clauses)
            {
                // based on the the current clause, instansiate the correct Lucene Query object
                var luceneQuery =
                    VisitableLuceneQueryFactory.Make(clause.Query);
                luceneQuery.Accept(this);
                switch (clause.Occur)
                {
                    case Occur.MUST:
                        must.Add(luceneQuery.ESQuery);
                        break;
                    case Occur.SHOULD:
                        should.Add(luceneQuery.ESQuery);
                        break;
                    case Occur.MUST_NOT:
                        mustNot.Add(luceneQuery.ESQuery);
                        break;
                }
            }

            boolClause.Must = must;
            boolClause.Should = should;
            boolClause.MustNot = mustNot;
            boolQueryWrapper.ESQuery = boolClause;
        }

        /// <summary>
        /// Throws exception if obj is not a valid LuceneQuery.
        /// </summary>
        /// <param name="obj">obj to verify.</param>
        protected static void VerifyValid(ILuceneVisitable obj)
        {
            if (obj == null)
            {
                throw new ArgumentException(
                    "Argument cannot be null");
            }

            if (obj.LuceneQuery == null)
            {
                throw new IllegalClauseException(
                    "LuceneQuery cannot be null");
            }
        }
    }
}