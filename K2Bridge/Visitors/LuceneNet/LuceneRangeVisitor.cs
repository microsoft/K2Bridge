// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors.LuceneNet
{
    using System.Globalization;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Models.Request.Queries.LuceneNet;
    using Lucene.Net.Search;

    /// <content>
    /// Defines a visit method for lucene range query.
    /// </content>
    internal partial class LuceneVisitor : ILuceneVisitor
    {
        /// <inheritdoc/>
        public void Visit(LuceneRangeQuery rangeQueryWrapper)
        {
            VerifyValid(rangeQueryWrapper);

            var rangeQuery = (TermRangeQuery)rangeQueryWrapper.LuceneQuery;
            var rangeClause = new RangeClause
            {
                FieldName = rangeQuery.Field,
                GTEValue = rangeQuery.LowerTerm,
                LTValue = rangeQuery.UpperTerm,
            };
            rangeQueryWrapper.ESQuery = rangeClause;
        }
    }
}
