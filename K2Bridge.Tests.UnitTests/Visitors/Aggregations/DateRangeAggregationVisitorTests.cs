// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Visitors.Aggregations;

using System.Collections.Generic;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Tests.UnitTests.Visitors;
using K2Bridge.Visitors;
using NUnit.Framework;

[TestFixture]
public class DateRangeAggregationVisitorTests
{
    [TestCase("2018-02-01", "2018-02-02", "2018-02-02", "2018-02-3", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array(strcat(make_datetime('2018-02-01'), '%', make_datetime('2018-02-02')),strcat(make_datetime('2018-02-02'), '%', make_datetime('2018-02-3'))), ['_range_value'] = pack_array(['timestamp'] >= make_datetime('2018-02-01') and ['timestamp'] < make_datetime('2018-02-02'),['timestamp'] >= make_datetime('2018-02-02') and ['timestamp'] < make_datetime('2018-02-3'))\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
    [TestCase(null, "2018-02-02", "2018-02-02", "2018-02-3", ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array(strcat('', '%', make_datetime('2018-02-02')),strcat(make_datetime('2018-02-02'), '%', make_datetime('2018-02-3'))), ['_range_value'] = pack_array(['timestamp'] < make_datetime('2018-02-02'),['timestamp'] >= make_datetime('2018-02-02') and ['timestamp'] < make_datetime('2018-02-3'))\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
    [TestCase("2018-02-01", "2018-02-02", "2018-02-02", null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array(strcat(make_datetime('2018-02-01'), '%', make_datetime('2018-02-02')),strcat(make_datetime('2018-02-02'), '%', '')), ['_range_value'] = pack_array(['timestamp'] >= make_datetime('2018-02-01') and ['timestamp'] < make_datetime('2018-02-02'),['timestamp'] >= make_datetime('2018-02-02'))\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
    [TestCase(null, "2018-02-02", "2018-02-02", null, ExpectedResult = "\nlet _extdata = _data\n| extend ['key'] = pack_array(strcat('', '%', make_datetime('2018-02-02')),strcat(make_datetime('2018-02-02'), '%', '')), ['_range_value'] = pack_array(['timestamp'] < make_datetime('2018-02-02'),['timestamp'] >= make_datetime('2018-02-02'))\n| mv-expand ['key'] to typeof(string), ['_range_value']\n| where ['_range_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key']\n| order by ['key'] asc;\n(_summarizablemetrics\n| order by ['key'] asc\n| as aggs);")]
    public string DateRangeVisit_WithAggregation_ReturnsValidResponse(string from1, string to1, string from2, string to2)
    {
        var rangeAggregation = new DateRangeAggregation()
        {
            Metric = "wibble",
            Field = "timestamp",
            Key = "key",
            Ranges = new List<DateRangeAggregationExpression>() {
                    new DateRangeAggregationExpression { Field = "timestamp", From = from1, To = to1 },
                    new DateRangeAggregationExpression { Field = "timestamp", From = from2, To = to2 },
                },
        };

        var visitor = new ElasticSearchDSLVisitor(SchemaRetrieverMock.CreateMockTimestampSchemaRetriever());
        VisitorTestsUtils.VisitRootDsl(visitor);
        visitor.Visit(rangeAggregation);

        return rangeAggregation.KustoQL;
    }
}
