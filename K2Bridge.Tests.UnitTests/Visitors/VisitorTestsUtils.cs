// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors
{
    using global::UnitTests.K2Bridge.Visitors;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Queries;
    using K2Bridge.Visitors;

    public static class VisitorTestsUtils
    {
        /// <summary>
        /// A helpler method to add the default DSL to visitor in order to
        /// Have the ISchemaRetriever initialised by the default visitor.
        /// </summary>
        /// <param name="visitor"></param>
        internal static void AddRootDsl(ElasticSearchDSLVisitor visitor)
        {
            var dsl = new ElasticSearchDSL
            {
                Query = new Query
                {
                    Bool = new BoolQuery(),
                },
                IndexName = "someindex",
            };
            visitor.Visit(dsl);
        }

        internal static ElasticSearchDSLVisitor CreateRootVisitor()
        {
            var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockSchemaRetriever());
            AddRootDsl(visitor);
            return visitor;
        }
    }
}
