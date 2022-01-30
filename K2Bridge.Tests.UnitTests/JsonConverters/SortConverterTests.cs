// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Models.Request;
using NUnit.Framework;

namespace K2Bridge.Tests.UnitTests.JsonConverters;

[TestFixture]
public class SortConverterTests
{
    private const string ValidSort = @"
            {
                ""timestamp"": {
                    ""order"": ""desc""
                }
            }";

    private const string EmptyOrderSort = @"
            {
                ""timestamp"": { }
            }";

    private static readonly SortClause ExpectedValidSort = new()
    {
        FieldName = "timestamp",
        Order = "desc",
    };

    private static readonly SortClause ExpectedEmptySort = new()
    {
        FieldName = "timestamp",
        Order = null,
    };

    private static readonly object[] AggregationTestCases = {
            new TestCaseData(ValidSort, ExpectedValidSort).SetName("JsonDeserializeObject_WithValidSort_DeserializedCorrectly"),
            new TestCaseData(EmptyOrderSort, ExpectedEmptySort).SetName("JsonDeserializeObject_WithEmptySort_DeserializedCorrectly"),
        };

    [TestCaseSource(nameof(AggregationTestCases))]
    public void TestAggregationConvertor(string queryString, object expected)
    {
        queryString.AssertJsonString((SortClause)expected);
    }
}
