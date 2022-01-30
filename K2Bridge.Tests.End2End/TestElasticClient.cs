// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End;

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
using NUnit.Framework;

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
        var invalids = Path.GetInvalidFileNameChars();
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
        TestContext.Progress.WriteLine($"Validating connection to {baseAddress}");
        using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        TestContext.Progress.WriteLine($"Validated connection");
        return new TestElasticClient(client, dumpFileName);
    }

    /// <summary>
    /// When comparing the Percentiles Payloads, we need to omit the values due to ticket #15795.
    /// </summary>
    /// <param name="parent">JSON element at which to start search.</param>
    public static void DeleteValuesToComparePercentiles(JToken parent)
    {
        var tokens = parent.SelectTokens("responses[*].aggregations..value");
        foreach (JValue v in tokens)
        {
            if (v.Type == JTokenType.String)
            {
                v.Value = "0";
            }
        }

        tokens = parent.SelectTokens("responses[*].aggregations..value_as_string");
        foreach (JValue v in tokens)
        {
            if (v.Type == JTokenType.Date)
            {
                v.Value = "2022-01-01T23:50:52.916+00:00";
            }
        }

        tokens = parent.SelectTokens("responses[*].aggregations..buckets..values");
        foreach (var v in tokens)
        {
            // Median percentile KeyValuePair has "50.0" as Key
            if (v.Type == JTokenType.Object)
            {
                v["50.0"] = 0;
            }
        }
    }

    /// <summary>
    /// Get all descendants tokens from a root token.
    /// </summary>
    public static IEnumerable<JToken> GetAllDescendantsTokens(JToken rootToken)
    {
        var toSearch = new Stack<JToken>(rootToken.Children());
        while (toSearch.Count > 0)
        {
            var inspected = toSearch.Pop();
            yield return inspected;
            foreach (var child in inspected)
            {
                toSearch.Push(child);
            }
        }
    }

    /// <summary>
    /// Make a Math.Round operation on all float values in a json payload.
    /// </summary>
    public static void RoundFloats(JToken parent, string jsonPath, int? digits = null)
    {
        if (!digits.HasValue)
        {
            return;
        }

        var rootToken = parent.SelectToken(jsonPath);
        foreach (JValue v in GetAllDescendantsTokens(rootToken).Where(x => x.Type == JTokenType.Float))
        {
            var originalMetricValue = (double)v.Value;
            var normalizedMetricValue = Math.Round(originalMetricValue, digits.Value);
            v.Value = normalizedMetricValue;
        }
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
    ///     "number" : "7.16.2",
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
    /// <param name="validateHighlight">Controls the validation of the highlight element.</param>
    /// <param name="roundingFloats">If value is specified make a round operation on all floats.</param>
    /// <returns>SearchAsync operation result.</returns>
    public async Task<JToken> MSearch(string indexName, string jsonQueryFile, bool validateHighlight = true, int? roundingFloats = null)
    {
        var query = JObject.Parse(File.ReadAllText(jsonQueryFile));

        using var request = new HttpRequestMessage(HttpMethod.Post, "_msearch");
        var payload = new StringBuilder();
        payload.AppendLine(FormattableString.Invariant($"{{\"index\":\"{indexName}\"}}"));
        payload.AppendLine(query.ToString(Formatting.None));
        request.Content = new StringContent(payload.ToString());
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-ndjson");
        var result = await JsonQuery(request);
        MaskSearchCommon(result, "responses[*].");

        MaskValue(result, "took");

        if (!validateHighlight)
        {
            DeleteValue(result, "responses[*].hits.hits[*].highlight");
        }

        // Backend query isn't something we want to compare since it's unique to K2
        DeleteValue(result, "responses[*]._backendQuery");

        // Ignore fields.hour_of_day (script_fields are currently no supported)
        DeleteValue(result, "responses[*].hits.hits[*].fields.hour_of_day");

        // Ignore these fields in aggregation buckets
        DeleteValue(result, "responses[*].aggregations.*.doc_count_error_upper_bound");
        DeleteValue(result, "responses[*].aggregations.*.sum_other_doc_count");

        // Normalize aggregate value (double) with fixed number of decimal
        NormalizeAggregateValue(result, "responses[*].aggregations..value");

        // Make a Math.Round on all floats values
        RoundFloats(result, "responses[*].aggregations", roundingFloats);

        // Delete _id and _version in top hits aggregation
        DeleteValue(result, "responses[*].aggregations..hits[*]._id");
        DeleteValue(result, "responses[*].aggregations..hits[*]._version");

        return result;
    }

    /// <summary>
    /// API operation for resolving indices (hitting the IndexList endpoint).
    /// </summary>
    /// <param name="optionalIndexToKeep">Optional input with index name to keep.
    /// if this is not null, all other index names will be removed and it will be
    /// normalized by removing the database name from kusto's db:table pair.</param>
    /// <returns><c>JToken</c> with parsed response.</returns>
    public async Task<JToken> Search(string optionalIndexToKeep = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/_resolve/index/*");

        var result = await JsonQuery(request);

        DeleteValue(result, $"aliases");
        DeleteValue(result, $"data_streams");

        if (!string.IsNullOrEmpty(optionalIndexToKeep))
        {
            NormalizeIndexNamesForIndexList(result, "indices[*].name", optionalIndexToKeep);
        }

        return result;
    }

    /// <summary>
    /// API operation for field capabilities search.
    /// </summary>
    /// <param name="indexName">Index name to query.</param>
    /// <param name="removeDynamicColumns">Remove dynamic columns, used for comparisons where one side doesn't generate dynamic columns.</param>
    /// <returns><c>JToken</c> with parsed response.</returns>
    public async Task<JToken> FieldCaps(string indexName, bool removeDynamicColumns = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/{indexName}/_field_caps?fields=*&ignore_unavailable=true&allow_no_indices=false");
        var result = await JsonQuery(request);

        // Use generic Elasticsearch type for geo_point
        ReplaceType(result, "geo_point", "object", false);

        // Remove extra fields returned by Elasticsearch (prefixed by _)
        var fields = (JObject)result.SelectToken($"$.fields");
        var removes = new List<string>();
        foreach (var (name, _) in fields)
        {
            if (name.StartsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                removes.Add(name);
            }

            // Remove all dynamic fields, since for this elastic doesn't create them so they shouldn't be compared.
            if (removeDynamicColumns && name.Contains('.', StringComparison.OrdinalIgnoreCase))
            {
                removes.Add(name);
            }
        }

        removes.ForEach(f => fields.Remove(f));

        ReplaceType(result, "text", "keyword", true);

        return result;
    }

    public async Task<JToken> Templates(string templateName)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"_cat/templates/{templateName}?format=json");
        var result = await JsonQuery(request);
        return result;
    }

    public HttpClient Client()
    {
        return client;
    }

    private static void MaskSearchCommon(JToken result, string searchBase)
    {
        MaskValue(result, $"{searchBase}took");
        MaskValue(result, $"{searchBase}_shards.total");
        MaskValue(result, $"{searchBase}_shards.successful");
        MaskValue(result, $"{searchBase}_shards.skipped");
        MaskValue(result, $"{searchBase}_shards.failed");
        MaskValue(result, $"{searchBase}hits.hits[*]._id");
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
        foreach (var v in tokens)
        {
            v.Parent.Remove();
        }
    }

    /// <summary>
    /// Normalize aggregate metric value with fixed number of decimals.
    /// </summary>
    /// <param name="parent">JSON element at which to start search.</param>
    /// <param name="jsonPath">JSONPath search pattern for metric values to normalize.</param>
    private static void NormalizeAggregateValue(JToken parent, string jsonPath)
    {
        var tokens = parent.SelectTokens(jsonPath);
        foreach (JValue v in tokens)
        {
            if (v.Type == JTokenType.Float)
            {
                var originalMetricValue = (double)v.Value;
                var normalizedMetricValue = originalMetricValue.ToString("F6", CultureInfo.InvariantCulture);
                v.Value = normalizedMetricValue;
            }
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
