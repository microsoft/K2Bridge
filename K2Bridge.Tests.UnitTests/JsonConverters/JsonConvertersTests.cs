// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using DeepEqual.Syntax;
    using Newtonsoft.Json;
    using NUnit.Framework;

    public partial class JsonConvertersTests
    {
        public static void TestQueryStringQueriesInternal<T>(string queryString, T expected)
        {
            var expectedRes = expected;
            var deserializedObj = JsonConvert.DeserializeObject<T>(queryString);
            Assert.IsTrue(expectedRes.IsDeepEqual(deserializedObj));
        }
    }
}