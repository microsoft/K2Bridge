// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using System.Globalization;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <summary>
    /// Defines a visit method for lucene range query.
    /// </summary>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        public void Visit(LuceneRangeQuery rangeQueryWrapper)
        {
            VerifyValid(rangeQueryWrapper);

            var rangeQuery = (TermRangeQuery)rangeQueryWrapper.LuceneQuery;
            var rangeClause = new RangeClause
            {
                FieldName = rangeQuery.Field,
                GTEValue = decimal.Parse(rangeQuery.LowerTerm, CultureInfo.InvariantCulture),
                LTValue = decimal.Parse(rangeQuery.UpperTerm, CultureInfo.InvariantCulture),
            };
            rangeQueryWrapper.ESQuery = rangeClause;
        }
    }
}
