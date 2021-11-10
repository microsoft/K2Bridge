// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using FluentAssertions.Json;
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
        public void CompareElasticKusto_WhenClusterInfo_ResponsesAreEquivalent()
        {
            var es = ESClient().ClusterInfo();
            var k2 = K2Client().ClusterInfo();
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning zero results")]
        public void CompareElasticKusto_WhenMSearchZeroResults_ResponsesAreEquivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_ZeroResults_Equivalent.json", minResults: 0);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query returning two results")]
        public void CompareElasticKusto_WhenMSearchTwoResults_ResponsesAreEquivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_TwoResults_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text filter")]
        public void CompareElasticKusto_WhenMSearchTextFilter_ResponsesAreEquivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana query with text substring")]
        public void CompareElasticKusto_WhenMSearchContains_ResponsesAreEquivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_Text_Contains.json");
        }

        [Test]
        [Description("/_msearch Kibana query with quotation text substring")]
        public void CompareElasticKusto_WhenMSearchQuotations_ResponsesAreEquivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_Quotation.json");
        }

        [Test]
        [Description("/_msearch Kibana query with numeric field")]
        public void CompareElasticKusto_WhenMSearchNumeric_ResponsesAreEquivalentWithoutHighlight()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_Numeric.json", validateHighlight: false);
        }

        // TODO: when bug https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1695 is fixed merge this test with
        // MSearch_Numeric_Equivalent_WithoutHighlight and remove "validateHighlight: false" from it.
        [Test]
        [Description("/_msearch Kibana query with numeric field")]
        [Ignore("Bug#1695")]
        public void CompareElasticKusto_WhenMSearchNumeric_ResponsesAreEquivalentWithHighlight()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_Numeric.json");
        }

        [Test]
        [Description("/_msearch Kibana query with text multiple words substring")]
        public void CompareElasticKusto_WhenMSearchTextContainsMultiple_ResponsesAreEquivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_Text_Contains_Multiple.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes prefix) filter")]
        public void CompareElasticKusto_WhenMSearchTextFilterPrefix_ResponsesAreEquivalent()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Prefix_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes wildcard) filter")]
        public void CompareElasticKusto_WhenMSearchTextFilterWildcard_ResponsesAreEquivalent()
        {
            // Note: ELK uses 'keyword' and 'text' data types for text values, wildcards behavior for
            // those can vary (depends on the way the data was indexed), in the common, default scenario,
            // wildcard will not allow spaces for keywords but will allow spaces for text.
            // In ADX there is only string data type, hence one approach was chosen, to include spaces.
            // In this test, we chose a query of form FieldName:Something*Something* because this particular
            // field is of type text rather than keyword, hence ES and K2 results will be the same.
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_Wildcard_Equivalent.json");
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes specific field) filter")]
        public void CompareElasticKusto_WhenMSearchTextFilterFieldSpecificEquivilent_ResponsesAreEquivalentWithoutHighlight()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_FieldSpecific_Equivalent.json", validateHighlight: false);
        }

        // TODO: when bug https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1681 is fixed merge this test with
        // MSearch_TextFilter_FieldSpecific_Equivalent_WithoutHighlight and remove "validateHighlight: false" from it.
        [Test]
        [Description("/_msearch Kibana aggregation query with text (includes specific field) filter")]
        [Ignore("Bug#1681")]
        public void CompareElasticKusto_WhenMSearchTextFilterFieldSpecificEquivilent_ResponsesAreEquivalentWithHighlight()
        {
            ParallelQuery(
                $"{FLIGHTSDIR}/MSearch_TextFilter_FieldSpecific_Equivalent.json", validateHighlight: false);
        }

        [Test]
        [Description("/_msearch Kibana aggregation query with multiple filters")]

        // TODO: fix timezone in bucketing and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1479
        // TODO: fix multiple highlights and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1681
        // TODO: fix numeric highlights and enable test
        // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1695
        [Ignore("Requires fixing issues 1479 and 1681 and 1695")]
        public void CompareElasticKusto_WhenMSearchMultipleFilters_ResponsesAreEquivalent()
        {
            ParallelQuery($"{FLIGHTSDIR}/MSearch_MultipleFilters_Equivalent.json");
        }

        // NB Timestamp sorting is already covered by other test cases
        [Test]
        [TestCase("MSearch_Sort_String.json")]
        [TestCase("MSearch_Sort_Double.json")]
        [Description("/_msearch sort attribute with various data types")]
        public void CompareElasticKusto_WhenMSearchSort_ResponsesAreEquivalent(string queryFileName)
        {
            ParallelQuery($"{FLIGHTSDIR}/{queryFileName}");
        }

        [Test]
        [Description("/_resolve/index Kibana query")]
        public void CompareElasticKusto_WhenSearch_ResponsesAreEquivalent()
        {
            var es = ESClient().Search($"{INDEX}");
            var k2 = K2Client().Search($"{KustoDatabase()}:{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void CompareElasticKusto_WhenFieldCapsWithoutDatabaseName_ResponsesAreEquivalent()
        {
            var es = ESClient().FieldCaps(INDEX);
            var k2 = K2Client().FieldCaps(INDEX);
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("/{index}/_field_caps field capabilities Kibana query")]
        public void CompareElasticKusto_WhenFieldCapsWithDatabaseName_ResponsesAreEquivalent()
        {
            var es = ESClient().FieldCaps(INDEX);
            var k2 = K2Client().FieldCaps($"{KustoDatabase()}%3A{INDEX}");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        [Test]
        [Description("_cat/templates/kibana_index_template Kibana 7 query")]
        public void CompareElasticKusto_WhenKibanaTemplateQuery_ResponsesAreEquivalent()
        {
            var es = ESClient().Templates("kibana_index_template");
            var k2 = K2Client().Templates("kibana_index_template");
            AssertJsonIdentical(k2.Result, es.Result);
        }

        private static void AssertJsonIdentical(JToken k2, JToken es)
        {
            k2.Should().BeEquivalentTo(es);
        }

        private void ParallelQuery(string esQueryFile, string k2QueryFile = null, int minResults = 1, bool validateHighlight = true)
        {
            var es = ESClient().MSearch(INDEX, esQueryFile, validateHighlight);
            var k2 = K2Client().MSearch(INDEX, k2QueryFile ?? esQueryFile, validateHighlight);
            var t = es.Result.SelectToken("responses[0].hits.total.value");
            Assert.IsTrue(t.Value<int>() >= minResults);
            AssertJsonIdentical(k2.Result, es.Result);
        }
    }
}
