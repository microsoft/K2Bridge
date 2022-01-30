// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using K2Bridge.Utils;

namespace K2Bridge.Visitors;

/// <content>
/// Helper function to build metadata query.
/// </content>
internal partial class ElasticSearchDSLVisitor : IVisitor
{
    /// <summary>
    /// Given a metadata dictionary, generates a Kusto QL statement creating the metadata table:
    /// datatable(['key']:string, ['value']:string) ['2', 'val1', '2', 'val2', '2', 'val3'] | as metadata;
    /// The table has two columns, key and value.
    /// The key is the aggregation key, e.g. '2'.
    /// The value is an expected bucket name, e.g. 'val1'.
    /// </summary>
    /// <param name="metadata">A metadata dictionary.</param>
    /// <returns>A string containing the Kusto QL statement.</returns>
    public static string BuildMetadataQuery(Dictionary<string, List<string>> metadata)
    {
        var query = new StringBuilder();

        query.Append($"{KustoQLOperators.NewLine}{KustoQLOperators.Datatable}({KustoTableNames.MetadataKey}:{KustoTableNames.MetadataColumnType}, {KustoTableNames.MetadataValue}:{KustoTableNames.MetadataColumnType}) [");

        var values = new List<string>();

        // Keys
        foreach (var (key, valueList) in metadata)
        {
            // Values
            foreach (var value in valueList)
            {
                values.Add($"'{key}',{value}");
            }
        }

        query.Append(string.Join(',', values));
        query.Append(']');
        query.Append($" | as {KustoTableNames.Metadata};");

        return query.ToString();
    }
}
