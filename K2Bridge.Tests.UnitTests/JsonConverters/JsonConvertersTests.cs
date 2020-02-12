// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using DeepEqual.Syntax;
    using global::K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;
    using NUnit.Framework;

    public partial class JsonConvertersTests
    {
        public static void TestQueryStringQueriesInternal(string queryString, object expected)
        {
            var expectedRes = (Query)expected;
            var deserializedObj = JsonConvert.DeserializeObject<Query>(queryString);
            Assert.IsTrue(expectedRes.IsDeepEqual(deserializedObj));
        }
    }
}