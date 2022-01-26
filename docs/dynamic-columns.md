# Dynamic Columns

In ADX (Kusto), columns of the type "dynamic" are special.  
They can contain within themselves arbitrarily nested data structures, that can be different from row to row.
Within Kusto queries, you can access dynamic fields by using the dot notation.

For example, given a table T with the colmun "dyn" the values:
```
{"a": 3, "b":5 }
{"a": 4, "c":6 }
{"a": 5, "b":7 }
```

You can access the fields like so:
```kusto
T | where dyn.b == 5 | project dyn.a
```

For more information, see the official documentation:
https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/scalar-data-types/dynamic

## In K2Bridge

### Generating the fields

K2Bridge supports dynamic columns when creating an index pattern, and will automatically create them when the pattern is created or updated.  
If you're upgrading from an older version, you might need to refresh the pattern.

K2Bridge will summarize the rows of each of the dynamic fields, and using the `buildschema` operator will create a schema that describes each of the nested fields and their types.

By default, kusto will summarize all of the rows in the table to create the schema.    
This can be ill-advised for large tables that are also varied in their schemas.

There are two configuration options that limit the amount of rows that are summarized:
* `settings.maxDynamicSamples` - When set, instead of summarizing all of the rows, kusto will sample this many rows, and build the schema from that sample. This means that the schema will be generated much faster, but at the cost of incompleteness.
* `settings.maxDynamicSamplesIngestionTimeHours` - When set, instead of summarizing all of the rows, kusto will only sample rows that are ingested 'x' hours ago, with 'x' being the value from this configuration.

### Using dynamic fields

![Index pattern screen with StormEvents table](images/dynamic%20columns%20index%20patterns.png)

As you can see in the image, the dynamic fields are shown inline with the other fields.
`StormSummary.Details.Description` for example, is a field of type string, nested within the dynamic column `StormSummary`.

Using dynamic fields is identical to using normal fields - you may search or filter for them in the discovery tab, or use them as parameters in visualizations.

![Dynamic fields in the discovery tab](images/dynamic%20fields%20in%20the%20discover%20tab.png)
![Dynamic fields in the visualize tab](images/dynamic%20fields%20in%20the%20visualize%20tab.png)

## Pitfalls

### Heterogeneous Fields

In kusto, a dynamic field can be of different type, different in each row.
In kibana, this is not supported, so when a field has more than one type, it will be folded into "string".

In the future, smarter combinations may be available, such as combining a field that can either be "double" or "long" to a numeric type rather than a string.

### Array Fields

Kusto dynamic fields can contain arrays within themselves.
The arrays can either be homogeneous, or heterogeneous.

In kibana there is no concept of array types, and according to the documentation, all fields are treated as homogeneous arrays.

Therefore, when building the schema, K2bridge ignores array types and uses them as regular types.

This works for some uses for K2bridge, but still may be the subject of bugs when doing some queries in kusto.

Currently this is a topic for future research, and issues and contributions are welcome.


