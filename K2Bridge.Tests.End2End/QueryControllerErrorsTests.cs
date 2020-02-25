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
        private const string ExpectedInvalidQueryResponse = @"{""responses"":[{""error"":{""root_cause"":[{""type"":""SemanticException"",""reason"":""Semanticerror:'letfromUnixTimeMilli=(t:long){datetime(1970-01-01)+t*1millisec};let_data=database(\""build7865\"").kibana_sample_data_flights|where(dayOfWehas\""1\"")and(timestamp>=fromUnixTimeMilli(1517954400000)andtimestamp<=fromUnixTimeMilli(1518127199999));(_data|summarizecount()bytimestamp=startofmonth(timestamp)|orderbytimestampasc|asaggs);(_data|orderbytimestampdesc|limit500|ashits)'hasthefollowingsemanticerror:'where'operator:Failedtoresolvescalarexpressionnamed'dayOfWe'."",""index_uuid"":""kibana_sample_data_flights"",""index"":""kibana_sample_data_flights""}],""type"":""QueryException"",""reason"":""Failedexecutingazuredataexplorerquery"",""phase"":""query""},""status"":500}]}";

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
            Replace("\n", string.Empty, StringComparison.OrdinalIgnoreCase);

        private void AssertJson(string expectedJsonString, JToken json)
        {
            // remove unnecessary chars from json strings.
            expectedJsonString = NormalizeChars(expectedJsonString);
            var assertString = NormalizeChars(json.ToString());

            // remove database name (specific to build) from json
            expectedJsonString = Regex.Replace(expectedJsonString, "build\\d+", "database");
            assertString = Regex.Replace(assertString, "build\\d+", "database");
            Assert.AreEqual(
                assertString,
                expectedJsonString,
                $"json {assertString} did not match expected {expectedJsonString}");
        }
    }
}