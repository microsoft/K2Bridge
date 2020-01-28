// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using FluentAssertions;
    using FluentAssertions.Json;
    using Kusto.Data;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    /// <summary>
    /// Parallel end-to-end tests loading data into Kusto and Elasticsearch and assuring
    /// that K2Bridge returns equivalent outputs to Elasticsearch.
    /// </summary>
    [TestFixture]
    public class ParallelApiTest : KustoTestBase
    {
        /// <summary>
        /// Ensure the JSON response at the API root (containing general cluster information)
        /// is equivalent.
        /// This is not actually required for K2Bridge functionality,
        /// but is a test for the generic passthrough functionality
        /// to the K2Bridge internal Elasticsearch.
        /// </summary>
        [Test]
        [Description("Cluster general info (at API root URL)")]
        public void ClusterInfo_Equivalent()
        {
            var es = ESClient().ClusterInfo();
            var k2 = K2Client().ClusterInfo();
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning zero results")]
        public void MSearch_ZeroResults_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_ZeroResults_Equivalent.json", minResults: 0);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning two results")]
        public void MSearch_TwoResults_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_TwoResults_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text filter")]
        public void MSearch_TextFilter_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes prefix) filter")]
        public void MSearch_TextFilter_Prefix_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Prefix_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes wildcard) filter")]
        public void MSearch_TextFilter_Wildcard_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Wildcard_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes specific field) filter")]
        public void MSearch_TextFilter_FieldSpecific_Equivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_FieldSpecific_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with multiple filters")]

        // TODO: fix timezone in bucketing and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1479
        // TODO: fix multiple highlights and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1481
        [Ignore("Requires fixing issues 1479 and 1481")]
        public void MSearch_MultipleFilters_Equivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_MultipleFilters_Equivalent.json");
        }

        // NB Timestamp sorting is already covered by other test cases
        [Test]
        [TestCase("MSearch_Sort_String.json")]
        [TestCase("MSearch_Sort_Double.json")]
        [Description("/_msearch sort attribute with various data types")]
        public void MSearch_Sort_Equivalent(string queryFileName)
        {
            ParallelQuery($"{FLIGHTSDIR}/{queryFileName}");
        }

        [Test]
        [Description("/_search index list Kibana query")]
        public void Search_Equivalent()
        {
            var es = ESClient().Search();
            var k2 = K2Client().Search($"{KustoDatabase()}:{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void FieldCaps_Equivalent_WithoutDatabaseName()
        {
            var es = ESClient().FieldCaps(INDEX);
            var k2 = K2Client().FieldCaps(INDEX);
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void FieldCaps_Equivalent_WithDatabaseName()
        {
            var es = ESClient().FieldCaps(INDEX);
            var k2 = K2Client().FieldCaps($"{KustoDatabase()}%3A{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        private static void AssertJsonIdentical(JToken k2, JToken es)
        {
            k2.Should().BeEquivalentTo(es);
        }

        private void ParallelQuery(string esQueryFile, string k2QueryFile = null, int minResults = 1)
        {
            var es = ESClient().MSearch(INDEX, esQueryFile);
            var k2 = K2Client().MSearch(INDEX, k2QueryFile ?? esQueryFile);
            var t = es.Result.SelectToken("responses[0].hits.total");
            Assert.IsTrue(t.Value<int>() >= minResults);
            AssertJsonIdentical(k2.Result, es.Result);
        }
    }
}
