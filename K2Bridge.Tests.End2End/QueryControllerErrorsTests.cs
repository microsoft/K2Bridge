// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class QueryControllerErrorsTests : KustoTestBase
    {
        private const string ExpectedInvalidQueryResponse = @"{
  ""responses"": [
        {
            ""error"": {
                ""root_cause"": [
                {
                    ""type"": ""SemanticException"",
                    ""reason"": ""Semantic error: 'where' operator: Failed to resolve scalar expression named 'dayOfWe'. Query: 'let _data = database(\""build7865\"").kibana_sample_data_flights | where (dayOfWe has \""1\"") and (timestamp >= unixtime_milliseconds_todatetime(1517954400000) and timestamp <= unixtime_milliseconds_todatetime(1518127199999));\n(_data | summarize count() by timestamp = startofmonth(timestamp)\n| order by timestamp asc | as aggs);\n(_data | order by timestamp desc | limit 500 | as hits)'"",
                    ""index_uuid"": ""kibana_sample_data_flights"",
                    ""index"": ""kibana_sample_data_flights""
                }
                ],
                ""type"": ""QueryException"",
                ""reason"": ""Failed executing azure data explorer query"",
                ""phase"": ""query""
            },
            ""status"": 500
        }
        ]
    }";

        [Test]
        [Description("Search with invalid query (wrong column name) returns an elastic error response")]
        public async Task MSearch_WithInvalidQuery_ReturnsElasticErrorResponse()
        {
            var response = await K2Client().MSearch(INDEX, $"{FLIGHTSDIR}/MSearch_Invalid_Query.json", false);
            AssertJson(ExpectedInvalidQueryResponse, response);
        }

        private static string NormalizeChars(string s) =>
            s.
            Replace("\\n", string.Empty, StringComparison.OrdinalIgnoreCase).
            Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).
            Replace("\r\n", string.Empty, StringComparison.OrdinalIgnoreCase).
            Replace("\n", string.Empty, StringComparison.OrdinalIgnoreCase);

        private void AssertJson(string expectedJsonString, JToken json)
        {
            // remove unnecessary chars from json strings.
            expectedJsonString = NormalizeChars(expectedJsonString);
            var assertString = NormalizeChars(json.ToString());

            // remove database name (specific to build) from json
            var databasePattern = new Regex(@"database\(\\""[\w]+\\""\)"); // Matches database(\\"build7865\\")
            var replacement = "database(\"A\")";
            expectedJsonString = databasePattern.Replace(expectedJsonString, replacement);
            assertString = databasePattern.Replace(assertString, replacement);
            Assert.AreEqual(
                assertString,
                expectedJsonString,
                $"json {assertString} did not match expected {expectedJsonString}");
        }
    }
}