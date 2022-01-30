// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet;

using System.Collections.Generic;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Models.Request.Queries.LuceneNet;
using Lucene.Net.Search;

/// <content>
/// Defines a visit method for lucene boolean query.
/// </content>
internal partial class LuceneVisitor : ILuceneVisitor
{
    /// <inheritdoc/>
    public void Visit(LuceneBoolQuery boolQueryWrapper)
    {
        VerifyValid(boolQueryWrapper);

        var boolClause = new BoolQuery();
        var must = new List<IQuery>();
        var should = new List<IQuery>();
        var mustNot = new List<IQuery>();
        var clauses = ((BooleanQuery)boolQueryWrapper.LuceneQuery).GetClauses();

        foreach (var clause in clauses)
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
    /// Throws exception if luceneBoolObj is not a valid LuceneQuery.
    /// </summary>
    /// <param name="luceneBoolObj">luceneBoolObj to verify.</param>
    protected static void VerifyValid(ILuceneVisitable luceneBoolObj)
    {
        Ensure.IsNotNull(luceneBoolObj, nameof(luceneBoolObj));
        EnsureClause.IsNotNull(luceneBoolObj.LuceneQuery, nameof(luceneBoolObj.LuceneQuery));
    }
}
