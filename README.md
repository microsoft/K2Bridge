# K2Bridge

K2Bridge is a solution that enables Kibana to use [Azure Data Explorer](https://azure.microsoft.com/en-us/services/data-explorer/) (ADX, or codename Kusto) as its backend database.

---

[![Build Status](https://dev.azure.com/csedevil/Kibana-kusto-bridge/_apis/build/status/K2bridge-Dev-CICD?branchName=master)](https://dev.azure.com/csedevil/Kibana-kusto-bridge/_build/latest?definitionId=169&branchName=master)
[![MIT license](https://img.shields.io/badge/license-MIT-brightgreen.svg)](http://opensource.org/licenses/MIT)

## Description

The K2Bridge solution is a proxy capable of communicating with the Kibana application and translate its queries to [KQL](https://docs.microsoft.com/en-us/azure/kusto/query/), the query language of the Azure Data Explorer service.
The solution currently targets the "Discover" tab in Kibana to enable users to quickly and interactively explore their data. It supports the filters and well as the search box in the screen with both simple term search and Lucene expressions.

## How does it work

![Architecture](./docs/images/architecture.png)

The K2Bridge is the endpoint exposed to clients and the one Kibana connects to. Internally, a small elasticsearch is being used to service metadata related requests (index-patterns, saved queries etc.). Note that no business data is actually saved in this internal instance and it can be considered as an implementation detail (could be removed in the future).
The bridge accept each request and redirects business (data) requests to ADX and metadata requests to the metadata store.

### Some differences to be aware of

1. The [searching](./docs/searching.md) documentation provides insights to the similarities and differences between Elasticsearch and ADX as Kibana data sources.

1. Each document in Elasticsearch has a unique id usually noted in the "_id" field. This isn't inherently available for data stored in ADX and because Kibana expects it,
K2Bridge generates a *random* number for this value. Please note that this is *not a reproducible* value and you shouldn't search for documents/items that have specific values.

1. We currently don't have a plan to support Visualize or Dashboards in Kibana but will be interested in your feedback regarding those missing features. Feel free to vote and/or comment on this [issue](../../issues/3).

1. We have used and tested the solution using Kibana OSS 7.10.2.

## Installing

K2Bridge deploys to Kubernetes. Instructions are available [here](./docs/installation.md).

## Connecting data

The application settings contains the credentials for a service principal, a
reference to an ADX cluster and a default database within the cluster (`adxDefaultDatabaseName`).

The application surfaces the following data from ADX as indexes into Kibana:

* **Tables** located in any database on the ADX cluster, regardless of the `adxDefaultDatabaseName` setting, provided the service principal has Viewer permissions on the table.
* **Functions** located in the `adxDefaultDatabaseName` database only, provided:
  * The service principal has Viewer permissions on the function.
  * The function does not take any parameters.

ADX functions without parameters are similar in nature to views in relational databases.
Through functions, you can perform preform calculations, joins as well as [cross-database and cross-cluster queries](https://docs.microsoft.com/en-us/azure/kusto/query/cross-cluster-or-database-queries) and queries into [Azure Monitor (Application Insights and Log Analytics)](https://docs.microsoft.com/en-us/azure/data-explorer/query-monitor-data), provided the service principal has adequate permissions on the external resources.
For example:

```kql
.create function MyAzureMonitorConnectionFunction() {
    cluster('https://ade.loganalytics.io/subscriptions/<subscription-id>/resourceGroups/<resource-group-name>/providers/Microsoft.OperationalInsights/workspaces/<workspace-name>')
    .database('<workspace-name>')
    .<tablename>
}
```

Make sure you grant access to the service principal (that you created as part of the [installation](./docs/installation.md) requirements) from your Log Analytics workspaces.
To do so, go to your Log Analytics resource on the Azure portal, click on Access Control (IAM) and then Add. Click "Add role assignment", set the Role to Reader, and select the service principal created earlier.

Be mindful of the performance impact of such distributed queries, which can easily result into Kibana timeouts.

## Prometheus Support

K2Bridge supports the Prometheus protocol for metrics reporting (like request time, query time and payload size).
Supported exposition formats are the 0.0.4 text and protocol buffer formats.

More on the formats can be found at the [Prometheus documentations](https://prometheus.io/docs/instrumenting/exposition_formats/).

K2Bridge would reply based on the content type header, so pointing your browser to:
`http://bridge-host/metrics/` will return a text representation of the metrics with their documentation.

## Performance

You can find more about the performance test and capabilities in the [Performance page](/performance/Performance.md).

## Developing

Information on how to run Kibana and K2Bridge locally for development and testing can be found [here](./docs/development.md).

## Data Collection

The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft's privacy statement. Our privacy statement is located at https://go.microsoft.com/fwlink/?LinkID=824704. You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.

Data collection may be disabled by installing the K2 helm chart by setting the collectTelemetry field to false.
e.g: '--set settings.collectTelemetry=false'

## Attribution

Elasticsearch is a trademark of Elasticsearch BV, registered in the U.S. and in other countries.
Kibana is a trademark of Elasticsearch BV, registered in the U.S. and in other countries.

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
