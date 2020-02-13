// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using static System.StringComparison;

    public class PrometheusTest : KustoTestBase
    {
        [Test]
        [Description("Expose exception counters")]
        public async Task PrometheusTelemetry_WhenExceptionLogged_ThenIncrementExceptionCounter()
        {
            using var request1 = new HttpRequestMessage(HttpMethod.Post, "_msearch");
            var payload = new StringBuilder();
            payload.AppendLine($"{{\"index\":\"dummy\"}}");
            payload.AppendLine("{\"badly_formatted_json\":}");
            request1.Content = new StringContent(payload.ToString());
            request1.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-ndjson");
            await K2Client().Client().SendAsync(request1);

            using var request = new HttpRequestMessage(HttpMethod.Get, "/metrics");
            var response = await K2Client().Client().SendAsync(request);
            var responseData = await response.Content.ReadAsStringAsync();

            Assert.True(
                responseData.Contains(@"exceptions_by_type{ExceptionType=""JsonReaderException"",SourceContext=""K2Bridge.Controllers.QueryController"",ActionName=""K2Bridge.Controllers.QueryController.SearchAsync (K2Bridge)""}", Ordinal) &&
                responseData.Contains(@"exceptions_by_type{ExceptionType=""JsonReaderException"",SourceContext=""K2Bridge.ElasticQueryTranslator"",ActionName=""K2Bridge.Controllers.QueryController.SearchAsync (K2Bridge)""}", Ordinal));

            Assert.True(
                responseData.Contains(
                "# HELP exceptions Exceptions logged\n"
                + "# TYPE exceptions counter\n"
                + "exceptions ",
                Ordinal));
        }

        [Test]
        [Description("Expose Kusto Net query execution time")]
        public async Task PrometheusTelemetry_WhenQueryParsed_ThenExposeQueryMetrics()
        {
            await K2Client().MSearch(INDEX, $"{FLIGHTSDIR}/MSearch_Sort_String.json");

            using var request = new HttpRequestMessage(HttpMethod.Get, "/metrics");
            var response = await K2Client().Client().SendAsync(request);
            var responseData = await response.Content.ReadAsStringAsync();

            var metrics = Telemetry.Metrics.Create();

            Assert.True(
                responseData.Contains(
                $"# HELP {metrics.AdxQueryDurationMetric.Name} {metrics.AdxQueryDurationMetric.Help}\n"
                + $"# TYPE {metrics.AdxQueryDurationMetric.Name} histogram\n",
                Ordinal), responseData);

            Assert.True(
                responseData.Contains(
                $"# HELP {metrics.AdxNetQueryDurationMetric.Name} {metrics.AdxNetQueryDurationMetric.Help}\n"
                + $"# TYPE {metrics.AdxNetQueryDurationMetric.Name} histogram\n",
                Ordinal), responseData);

            Assert.True(
                responseData.Contains(
                $"# HELP {metrics.AdxQueryBytesMetric.Name} {metrics.AdxQueryBytesMetric.Help}\n"
                + $"# TYPE {metrics.AdxQueryBytesMetric.Name} histogram\n",
                Ordinal), responseData);
        }
    }
}
