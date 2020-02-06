// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests.Visitors
{
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;
    using NUnit.Framework;
    using Tests;

    [TestFixture]
    public class TestExistsVisitor
    {
        [TestCase(ExpectedResult = "isnotnull(MyField)")]
        public string TestValidExistsVisit()
        {
            var existsClause = new ExistsClause
            {
                FieldName = "MyField",
            };

            var visitor = new ElasticSearchDSLVisitor(LazySchemaRetrieverMock.CreateMockSchemaRetriever());
            visitor.Visit(existsClause);
            return existsClause.KustoQL;
        }
    }
}