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

This aggregation is mapped to :
- [variancep()](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/variancep-aggfunction)
- [variance()](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/variance-aggfunction)
- [stdevp()](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/stdevp-aggfunction)
- [stdev()](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/stdev-aggfunction)

The Standard Deviation Bounds values are calculated internally by K2Bridge:

```
Lower Deviation (Population or Sampling) = Average Value - (Standard Deviation (Population or Sampling) * Sigma)
Upper Deviation (Population or Sampling) = Average Value + (Standard Deviation (Population or Sampling) * Sigma)
```

The `Sigma` value controls controls how many standard deviations +/- from the mean should be displayed (see explanation on how to use it, below).

> As reference, you can check the Elasticsearch implementation used to calculate bounds here : [getStdDeviationBound()](https://github.com/elastic/elasticsearch/blob/0699c9351f1439e246d408fd6538deafde4087b6/server/src/main/java/org/elasticsearch/search/aggregations/metrics/InternalExtendedStats.java#L187-L194)

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2007-01-27T15:18:02.1310000Z") and ['timestamp'] <= todatetime("2022-01-27T15:18:02.1310000Z"));

let _extdata = _data
| extend ['f4674a57-bb73-4916-8a3f-c0bb65ee4c0d'] = true;

let _summarizablemetrics = _extdata
| summarize ['1%extended_stats%2']=pack('count', count(),'min', min(['AvgTicketPrice']),'max', max(['AvgTicketPrice']),'avg', avg(['AvgTicketPrice']),'sum', sum(['AvgTicketPrice']),'sum_of_squares', sum(pow(['AvgTicketPrice'], 2)),'variance_population', variancep(['AvgTicketPrice']),'variance_sampling', variance(['AvgTicketPrice']),'std_deviation_population', stdevp(['AvgTicketPrice']),'std_deviation_sampling', stdev(['AvgTicketPrice'])),count() by ['f4674a57-bb73-4916-8a3f-c0bb65ee4c0d'];

(_summarizablemetrics
| project-away ['f4674a57-bb73-4916-8a3f-c0bb65ee4c0d']
| as aggs);
```
## Sigma

The `Sigma` value is not used from within the ADX query and is calculated later on by K2Bridge.
`Sigma` can be any non-negative double, meaning you can request non-integer values such as `3`.
A value of `0` is valid, but will simply return the average for both upper and lower bounds.

``` json
{
  "size": 0,
  "aggs": {
    "grades_stats": {
      "extended_stats": {
        "field": "grade",
        "sigma": 3          
      }
    }
  }
}
```
You can also add the sigma value, directly in the Advanced JSON input field. The value will be merged with the elasticsearch aggregation definition:

![sigma](../images/sigma.png)


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

> **_Estimation error in percentiles:_**  
The percentiles aggregate provides an approximate value using [T-Digest](https://github.com/tdunning/t-digest/blob/master/docs/t-digest-paper/histo.pdf).
>
> * The bounds on the estimation error vary with the value of the requested percentile. The best accuracy is at both ends of the [0..100] scale. Percentiles 0 and 100 are the exact minimum and maximum values of the distribution. The accuracy gradually decreases towards the middle of the scale. It's worst at the median and is capped at 1%.
> * Error bounds are observed on the rank, not on the value. Suppose percentile(X, 50) returned a value of Xm. The estimate guarantees that at least 49% and at most 51% of the values of X are less or equal to Xm. There is no theoretical limit on the difference between Xm and the actual median value of X.
> * The estimation may sometimes result in a precise value but there are no reliable conditions to define when it will be the case.

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

In the current implementation, when used within visualization chart, this aggregation is similar to [Top metrics aggregation](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-top-metrics.html). The top_metrics aggregation selects metrics from the document with the largest or smallest "sort" value. 

For each bucket, this metric returns the first or last N records by the selected `sort on` value. N is specified by the `size` value. And, from these returned N records, we project the `field` value. If you have more that one record returned, you can apply an additional aggregation (for instance sum N `field` values).

<img src="images/top_hits.png" width="40%">

This aggregation is mapped on [top operator](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/topoperator) when translated to Kusto Query Language.

Compared to others metrics aggregation, top hits cannot be translated in Kusto with the help of [summarize operator](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/summarizeoperator). An additional sub query is performed for each top hits metric requested.

As we need to select top hits for each individual buckets requested, [top operator](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/topoperator) expression is embedded inside a [partition](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/partitionoperator). The partition operator partitions the records of its input table into multiple subtables according to values in a key column, runs a subquery on each subtable, and produces a single output table that is the union of the results of all subqueries.

Example of Kusto Query Language built by K2Bridge:

```
let _data = kibana_data_flights
| where (['timestamp'] >= todatetime("2018-02-01T11:00:00.0000000Z") and ['timestamp'] <= todatetime("2018-02-02T11:00:00.0000000Z"));

let _extdata = _data
| extend ['9e36322f-9696-49ae-ad3b-6b8158b76b75'] = true;

let _summarizablemetrics = _extdata
| summarize count() by ['9e36322f-9696-49ae-ad3b-6b8158b76b75'];

let _tophits1 = _extdata
| join kind=inner _summarizablemetrics on ['9e36322f-9696-49ae-ad3b-6b8158b76b75']
| partition by ['9e36322f-9696-49ae-ad3b-6b8158b76b75']
(
top 1 by ['timestamp'] desc
| project ['9e36322f-9696-49ae-ad3b-6b8158b76b75'],['count_'], ['1']=pack('source_field','DistanceKilometers','source_value',['DistanceKilometers'],'sort_value',['timestamp'])
| summarize take_any(['9e36322f-9696-49ae-ad3b-6b8158b76b75']),take_any(['count_']), ['1%tophits']=make_list(['1'])
);

(_tophits1 
| project-away ['9e36322f-9696-49ae-ad3b-6b8158b76b75']
| as aggs);
```
