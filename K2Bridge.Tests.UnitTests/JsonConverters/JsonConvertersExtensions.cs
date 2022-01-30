// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.JsonConverters;

using System;
using DeepEqual.Syntax;
using Newtonsoft.Json;
using NUnit.Framework;

internal static class JsonConvertersExtensions
{
    public static void AssertJsonString<T>(this string jsonString, T expected)
    {
        var deserializedObj = JsonConvert.DeserializeObject<T>(jsonString);
        Assert.IsTrue(expected.IsDeepEqual(deserializedObj), $"string {expected} did not match {deserializedObj}");
    }

    public static void AssertJson<T>(this T json, string expected)
    {
        var serializedString = JsonConvert.SerializeObject(json);
        Assert.IsTrue(
            expected.NormalizeChars().Equals(
                serializedString.NormalizeChars(),
                StringComparison.OrdinalIgnoreCase),
            $"json {expected.NormalizeChars()} did not match {serializedString.NormalizeChars()}");
    }

    private static string NormalizeChars(this string s)
    {
        return s.
            Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).
            Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase).
            Replace("\n", string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
