// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A low-level client for Elasticsearch operations.
    /// Exposes the Elasticsearch API operations under test, replacing
    /// non-repeatable content (such as cluster name) with placeholders.
    /// Used by parallel tests to query both the Elasticsearch backend
    /// and the K2Bridge backend with identical queries and assert
    /// that the results are identical (after placeholder replacement).
    /// </summary>
    public class TestElasticClient
    {
        private readonly HttpClient client;

        private readonly string dumpFileName;

        private TestElasticClient(HttpClient client, string dumpFileName)
        {
            this.client = client;
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var safeName = string.Join("_", dumpFileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            this.dumpFileName = safeName;
        }

        public static async Task<TestElasticClient> Create(string baseAddress, string dumpFileName)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
            };

            // validate backend host is reachable
            using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return new TestElasticClient(client, dumpFileName);
        }

        /// <summary>
        /// Queries the backend and parses the result as JSON.
        /// </summary>
        /// <param name="request">Request to backend.</param>
        /// <returns>Backend response as parsed JSON.</returns>
        public async Task<JToken> JsonQuery(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);
            var result = await ParseJsonResponse(response);
            return result;
        }

        /// <summary>
        /// API operation for getting cluster information.
        /// </summary>
        /// <remarks>
        /// GET call at the Elasticsearch root path.
        ///
        /// Sample response after replacing variable items with the
        /// <c>__placeholder__</c> string:
        /// <c>
        /// {
        ///   "name" : "__placeholder__",
        ///   "cluster_name" : "__placeholder__",
        ///   "cluster_uuid" : "__placeholder__",
        ///   "version" : {
        ///     "number" : "6.8.5",
        ///     "build_flavor" : "oss",
        ///     "build_type" : "docker",
        ///     "build_hash" : "78990e9",
        ///     "build_date" : "2019-11-13T20:04:24.100411Z",
        ///     "build_snapshot" : false,
        ///     "lucene_version" : "7.7.2",
        ///     "minimum_wire_compatibility_version" : "5.6.0",
        ///     "minimum_index_compatibility_version" : "5.0.0"
        ///   },
        ///   "tagline" : "You Know, for Search"
        /// }
        /// </c>.
        /// </remarks>
        /// <returns>Cluster general information.</returns>
        public async Task<JToken> ClusterInfo()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
            var result = await JsonQuery(request);
            MaskValue(result, "name");
            MaskValue(result, "cluster_name");
            MaskValue(result, "cluster_uuid");
            return result;
        }

        /// <summary>
        /// API operation to run multiple searches. Client is configured to run
        /// a single time histogram query, as run by Kibana.
        /// </summary>
        /// <param name="indexName">Index name to query.</param>
        /// <param name="jsonQueryFile">File name containing query.</param>
        /// <returns>Search operation result.</returns>
        public async Task<JToken> MSearch(string indexName, string jsonQueryFile)
        {
            JObject query = JObject.Parse(File.ReadAllText(jsonQueryFile));

            using var request = new HttpRequestMessage(HttpMethod.Post, "_msearch");
            var payload = new StringBuilder();
            payload.AppendLine($"{{\"index\":\"{indexName}\"}}");
            payload.AppendLine(query.ToString(Formatting.None));
            request.Content = new StringContent(payload.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-ndjson");
            var result = await JsonQuery(request);
            MaskSearchCommon(result, "responses[*].");

            // TODO: returned by ES but not by K2
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1468
            DeleteValue(result, "responses[*].hits.hits[*].fields");

            // TODO: returned by ES but not by K2
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1468
            DeleteValue(result, "responses[*].hits.hits[*].sort");

            // TODO: returned by K2 but not by ES
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1468
            // TODO: Fix substringhighlighting
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1481
            DeleteValue(result, "responses[*].hits.hits[*].highlight");

            foreach (JToken token in result.SelectTokens("$..*"))
            {
                // TODO: Kusto does not preserve all 32 bit range, so we compare only first 16 bits of decimal values
                // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1466
                if (token.Type == JTokenType.Float)
                {
                    var v = (JValue)token;
                    v.Value = Convert.ToSingle(v.Value, CultureInfo.InvariantCulture);
                }

                // TODO: K2 returns boolean as int
                // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1463
                if (token.Type == JTokenType.Boolean)
                {
                    var v = token as JValue;
                    v.Value = Convert.ToInt16(v.Value, CultureInfo.InvariantCulture);
                }
            }

            // TODO: distinct timestamp formats for aggregation keys
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1464
            NormalizeTimestamps(result, "responses[*].aggregations.*.buckets[*].key_as_string");

            // TODO: distinct timestamp formats for timestamp attributes
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1487
            DeleteValue(result, "responses[*].hits.hits[*]._source.timestamp");

            return result;
        }

        /// <summary>
        /// API operation for wildcard index search (hitting the IndexList endpoint).
        /// </summary>
        /// <param name="optionalIndexToKeep">Optional input with index name to keep.
        /// if this is not null, all other index names will be removed and it will be
        /// normalized by removing the database name from kusto's db:table pair.</param>
        /// <returns><c>JToken</c> with parsed response.</returns>
        public async Task<JToken> Search(string optionalIndexToKeep = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/*/_search/")
            {
                Content = new StringContent("{\"size\":0,\"aggs\":{\"indices\":{\"terms\":{\"field\":\"_index\",\"size\":200}}}}"),
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var result = await JsonQuery(request);

            MaskSearchCommon(result, string.Empty);

            // TODO: K2Bridge always returns 0 for doc_count
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1467
            MaskValue(result, "aggregations.indices.buckets[*].doc_count");

            // TODO: K2Bridge always returns 0 for total
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1467
            MaskValue(result, "hits.total");

            // TODO: K2Bridge returns null
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1467
            MaskValue(result, "hits.max_score");

            // TODO: K2Bridge returns extra status field
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1467
            DeleteValue(result, "status");

            // TODO: K2Bridge does not return these two fields
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1467
            DeleteValue(result, "aggregations.indices.doc_count_error_upper_bound");
            DeleteValue(result, "aggregations.indices.sum_other_doc_count");
            if (!string.IsNullOrEmpty(optionalIndexToKeep))
            {
                NormalizeIndexNamesForIndexList(result, "aggregations.indices.buckets[*].key", optionalIndexToKeep);
            }

            return result;
        }

        /// <summary>
        /// API operation for field capabilities search.
        /// </summary>
        /// <param name="indexName">Index name to query.</param>
        /// <returns><c>JToken</c> with parsed response.</returns>
        public async Task<JToken> FieldCaps(string indexName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{indexName}/_field_caps?fields=*&ignore_unavailable=true&allow_no_indices=false");
            var result = await JsonQuery(request);

            // Use generic Elasticsearch type for geo_point
            ReplaceType(result, "geo_point", "object", false);

            // Remove extra fields returned by Elasticsearch (prefixed by _)
            JObject fields = (JObject)result.SelectToken($"$.fields");
            var removes = new List<string>();
            foreach (var field in fields)
            {
                if (field.Key.StartsWith("_", StringComparison.Ordinal))
                {
                    removes.Add(field.Key);
                }
            }

            removes.ForEach(f => fields.Remove(f));

            ReplaceType(result, "text", "keyword", true);

            return result;
        }

        private static void MaskSearchCommon(JToken result, string searchBase)
        {
            MaskValue(result, $"{searchBase}took");
            MaskValue(result, $"{searchBase}_shards.total");
            MaskValue(result, $"{searchBase}_shards.successful");
            MaskValue(result, $"{searchBase}_shards.skipped");
            MaskValue(result, $"{searchBase}_shards.failed");
            MaskValue(result, $"{searchBase}hits.hits[*]._id");

            // TODO: remove backendQuery if not enabled
            // https://dev.azure.com/csedevil/K2-bridge-internal/_workitems/edit/1461
            DeleteValue(result, $"{searchBase}backendQuery");
        }

        /// <summary>
        /// Replaces the occourances that meets jsonPath and value is equal to indexName
        /// by removing the database name from the pair databasename:tablemame.
        /// Any other occurrence whose value does not equal indexName is removed.
        /// </summary>
        /// <param name="parent">JSON element at which to start search.</param>
        /// <param name="jsonPath">JSONPath search pattern to replace.</param>
        /// <param name="indexName">index Name to keep and normalize.</param>
        private static void NormalizeIndexNamesForIndexList(JToken parent, string jsonPath, string indexName)
        {
            var tokens = parent.SelectTokens(jsonPath).Where(j => j is JValue).Select(j => j as JValue).ToArray();
            for (var index = 0; index < tokens.Length; index++)
            {
                if (tokens[index].Value.Equals(indexName))
                {
                    var tableName = indexName.Split(':')[1];
                    tokens[index].Value = tableName;
                }
                else
                {
                    tokens[index].Parent.Parent.Remove();
                }
            }
        }

        /// <summary>
        /// Replaces values designated by a JSONPath search pattern with a placeholder string.
        /// </summary>
        /// <param name="parent">JSON element at which to start search.</param>
        /// <param name="jsonPath">JSONPath search pattern to replace.</param>
        private static void MaskValue(JToken parent, string jsonPath)
        {
            var tokens = parent.SelectTokens(jsonPath);
            foreach (JValue v in tokens)
            {
                v.Value = "__placeholder__";
            }
        }

        /// <summary>
        /// Deletes values designated by a JSONPath search pattern with a placeholder string.
        /// </summary>
        /// <param name="parent">JSON element at which to start search.</param>
        /// <param name="jsonPath">JSONPath search pattern to delete.</param>
        private static void DeleteValue(JToken parent, string jsonPath)
        {
            var tokens = parent.SelectTokens(jsonPath);
            foreach (JToken v in tokens)
            {
                v.Parent.Remove();
            }
        }

        /// <summary>
        /// Normalize timestamp timezones to UTC.
        /// </summary>
        /// <param name="parent">JSON element at which to start search.</param>
        /// <param name="jsonPath">JSONPath search pattern for DateTime values to normalize.</param>
        private static void NormalizeTimestamps(JToken parent, string jsonPath)
        {
            var tokens = parent.SelectTokens(jsonPath);
            foreach (JValue v in tokens)
            {
                var originalTimestamp = (DateTime)v.Value;
                var normalizedTimestamp = TimeZoneInfo.ConvertTimeToUtc(originalTimestamp);
                v.Value = normalizedTimestamp.ToString("o", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Utility method for <c>FieldCaps</c>: substitute a type for another type in
        /// <c>fields</c> response.
        /// </summary>
        /// <param name="parent">JSON element at which to start search.</param>
        /// <param name="srcType">Source type to be replaced.</param>
        /// <param name="dstType">Destination type to be introduced.</param>
        /// <param name="setAggregatable">If true, sets type to aggregatable.</param>
        private static void ReplaceType(JToken parent, string srcType, string dstType, bool setAggregatable)
        {
            foreach (JObject v in parent.SelectTokens($"$.fields.*.{srcType}"))
            {
                var ancestor = (JObject)v.Parent.Parent;
                ancestor.Remove(srcType);
                ancestor[dstType] = v;
                ((JValue)ancestor[dstType]["type"]).Value = dstType;
                if (setAggregatable)
                {
                    ((JValue)ancestor[dstType]["aggregatable"]).Value = true;
                }
            }
        }

        /// <summary>
        /// Ensures an HTTP call was successful and parse its response message into JSON.
        /// </summary>
        /// <param name="response">The HTTP response from <c>HttpClient</c>.</param>
        /// <returns><c>JToken</c> with parsed response.</returns>
        private async Task<JToken> ParseJsonResponse(HttpResponseMessage response)
        {
            var responseData = await response.Content.ReadAsStringAsync();

            // Save a copy of the response locally to help debugging
            File.WriteAllText(dumpFileName, responseData);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{response.StatusCode} {response.ReasonPhrase} at {client.BaseAddress}: <{responseData}>");
            }

            var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None }
;
            _ = JsonConvert.DeserializeObject<JToken>(responseData, settings); // data
            var actual = JToken.Parse(responseData);
            return actual;
        }
    }
}
