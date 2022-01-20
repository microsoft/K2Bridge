// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class ExistsVisitorTests
    {
        [TestCase(ExpectedResult = "isnotnull(['MyField'])")]
        public string ExistsVisit_WithValidInput_ReturnsIsNotNullResponse()
        {
            var existsClause = new ExistsClause
            {
                FieldName = "MyField",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(existsClause);
            return existsClause.KustoQL;
        }

        [TestCase(ExpectedResult = "isnotnull(['MyField'].['@1'].['b'])")]
        public string ExistsVisit_WithValidDynamicInput_ReturnsIsNotNullResponse()
        {
            var existsClause = new ExistsClause
            {
                FieldName = "MyField.@1.b",
            };

            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(existsClause);
            return existsClause.KustoQL;
        }
    }
}
