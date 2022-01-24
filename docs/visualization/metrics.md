# Average aggregation

A single-value metrics aggregation that computes the average of numeric values that are extracted from the aggregated documents. These values can be extracted either from specific numeric fields in the documents.

[Avg aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-avg-aggregation.html)

This aggregation is mapped on [avg() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/avg-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2']=avg(['DistanceKilometers']),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Count aggregation

This aggregation is mapped on [count() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/count-aggfunction) when translated to Kusto Query Language.

Note: Total count of documents is always returns to Kibana as part of Hits collection information. The selection of count aggregation in Kibana UI displays or hides this information, but it has no impact on the query built in Kusto. A dedicated table named `hitsTotal` is used by K2Bridge to get this information.

In addition, when bucket aggregation is performed, we include systematically count() function in the Kusto query to catch the document count of each bucket.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Cardinality aggregation

A single-value metrics aggregation that calculates an approximate count of distinct values.

[Cardinality aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-cardinality-aggregation.html)

This aggregation is mapped on [dcount() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/dcount-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2']=dcount(['DistanceKilometers']),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Extended stats aggregation

A multi-value metrics aggregation that computes stats over numeric values extracted from the aggregated documents.

[Extended stats aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-extendedstats-aggregation.html)

# Max aggregation

A single-value metrics aggregation that keeps track and returns the maximum value among the numeric values extracted from the aggregated documents.

[Max aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-max-aggregation.html)

This aggregation is mapped on [max() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/max-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2']=max(['DistanceKilometers']),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Min aggregation

A single-value metrics aggregation that keeps track and returns the minimum value among numeric values extracted from the aggregated documents.

[Min aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-min-aggregation.html)

This aggregation is mapped on [min() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/min-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2']=min(['DistanceKilometers']),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Percentiles aggregation

A multi-value metrics aggregation that calculates one or more percentiles over numeric values extracted from the aggregated documents.

[Percentiles aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-percentile-aggregation.html)

This aggregation is mapped on [percentile_array() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/percentiles-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2%percentile%1.0%5.0%25.0%50.0%75.0%95.0%99.0%False']=percentiles_array(['DistanceKilometers'], 1,5,25,50,75,95,99),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

For this aggregation, some metadata are passed in the column name to build the response. It includes percentile values requested and if response must be [Keyed](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-percentile-aggregation.html#_keyed_response_6). We plan to move these metadata into a dedicated table in a future release.

Median aggregation is just a specific version of percentiles aggregation where value targeted is 50.

# Sum aggregation

A single-value metrics aggregation that sums up numeric values that are extracted from the aggregated documents. 

[Sum aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-sum-aggregation.html)

This aggregation is mapped on [sum() (aggregation function)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/sum-aggfunction) when translated to Kusto Query Language.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize ['2']=sum(['DistanceKilometers']),count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

(_summarizablemetrics 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] 
| as aggs);
```

# Top hits aggregation

A top_hits metric aggregator keeps track of the most relevant document being aggregated. This aggregator is intended to be used as a sub aggregator, so that the top matching documents can be aggregated per bucket.

[Top hits aggregation (Elasticsearch)](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-top-hits-aggregation.html)