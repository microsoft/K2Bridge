// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using DeepEqual.Syntax;
    using Newtonsoft.Json;
    using NUnit.Framework;

    internal static class JsonConvertersExtensions
    {
        public static void AssertJsonString<T>(this string queryString, T expected)
        {
            var deserializedObj = JsonConvert.DeserializeObject<T>(queryString);
            Assert.IsTrue(expected.IsDeepEqual(deserializedObj));
        }
    }
}