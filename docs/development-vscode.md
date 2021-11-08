# Development with Visual Studio Code

## Development Container

This repo provides a container that can be used directly for local development. If you never used a development container, we suggest reading first the following documentation for system requirements and installation steps.

[Developing inside a Container](https://code.visualstudio.com/docs/remote/containers)

This container is based on image `mcr.microsoft.com/vscode/devcontainers/base:0-ubuntu20.04` which already contains a set of dev tools and linux packages. 
For a complete list of tools and packages versions; you can check the [image history](https://github.com/microsoft/vscode-dev-containers/tree/main/containers/ubuntu/history) documentation.

The following packages are installed on top of this base image.

| Tool / library | Version |
|----------------|---------|
| terraform | 1.0.9 |
| azure-cli | latest |
| helm | latest |
| dotnet-sdk-3.1 | latest |
| dotnet-sdk-5.0 | latest |
| default-jre | latest |
| elasticsearch-oss| 6.8.20 |
| kibana-oss | 6.8.20 |

Elasticsearch and Kibana are available at `/home/vscode`; and dev container will start using vscode non-root user. For reference, you cannot start elasticsearch using root user, an exception will occur.

## Prerequisites - Azure Data Explorer

K2Bridge requires an access to [Azure Data Explorer](https://azure.microsoft.com/en-us/services/data-explorer/). You can create Data Explorer cluster and database manually -or- use `create-adx.sh` script to deploy resources.
Following steps are performed by this script: 

- [x] Create resource group
- [x] Create Azure Data Explorer cluster
- [x] Create Azure Data Explorer database
- [x] Create service principal
- [x] Assign service principal to database permissions (Viewer role)
- [x] Add environment variables to .bashrc (will be used by end-to-end tests)

Change directory to .devcontainer
```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd .devcontainer/
```
Login with Azure CLI
```bash
vscode ➜ /workspaces/K2Bridge/.devcontainer (feature/devcontainer ✗) $ az login
The default web browser has been opened at https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize. Please continue the login in the web browser. If no web browser is available or if the web browser fails to open, use device code flow with `az login --use-device-code`.
[...]
```
Ensure your default account is the correct one
```bash
vscode ➜ /workspaces/K2Bridge/.devcontainer (feature/devcontainer ✗) $ az account show
```
Execute create-adx.sh with a unique name.
This unique name will be used in resource group, cluster and service principal names. If needed, you can change default naming convention directly in create-adx.sh.

- RESOURCE_GROUP_NAME="rg-k2-$unique-dev"
- ADX_CLUSTER_NAME="adxk2$unique"
- ADX_DB_NAME="devdatabase"
- SERVICE_PRINCIPAL_NAME="sp-k2-$unique"

```bash
vscode ➜ /workspaces/K2Bridge/.devcontainer (feature/devcontainer ✗) $ ./create-adx.sh -u <uniquename>
[...]
Use following settings/secrets in appsettings.development.json:
aadClientId: 00000000-0000-0000-0000-000000000000
aadClientSecret: tuUuUuUuU-JXx~x~xxxxxxxxxxxJ00000_
aadTenantId: 00000000-0000-0000-0000-000000000000
adxClusterUrl: https://adxk2<uniquename>.westeurope.kusto.windows.net
adxDefaultDatabaseName: devdatabase
```

## K2Bridge - Application Settings

Create a new settings file and name it *K2Bridge/appsettings.development.json*, and configuration as demonstrated in [appsettings.json](../K2Bridge/appsettings.json). Values can be retrieved from previous script output.

Below you can see a common settings file suitable for development:

```json
{
    "aadClientId": "<aadClientId>",
    "aadClientSecret": "<aadClientSecret>",
    "aadTenantId": "<aadTenantId>",
    "adxClusterUrl": "<adxClusterUrl>",
    "adxDefaultDatabaseName": "<adxDefaultDatabaseName>",
    "bridgeListenerAddress": "http://localhost:8080", //this needs to be identical to what kibana will connect to
    "metadataElasticAddress": "http://localhost:9200",
    "outputBackendQuery": "true",
    "collectTelemetry": "false", //unless you want to work on app-insights
    "enableQueryLogging": "true",

    //this section overrides default Serilog configuration to make it easier to develop and see logs.
    "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Filters.Expressions" ],
    "MinimumLevel": {
        "Default": "Verbose",
        "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Serilog.AspNetCore.RequestLoggingMiddleware": "Warning",
        "System": "Warning"
        }
    },
    "WriteTo": [
        {
        "Name": "Console",
        "Args": {
            "outputTemplate": "[{Timestamp:o} {CorrelationId} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
        }
    ],
    "Enrich": [ "FromLogContext", "WithCorrelationId", "WithCorrelationIdHeader" ]
    }
}
```

## Launch Steps

### 1. Start Elastic Search

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-6.8.20  kibana-6.8.20-linux-x86_64
vscode ➜ ~ $ cd elasticsearch-6.8.20/
vscode ➜ ~/elasticsearch-6.8.20 $ ./bin/elasticsearch

[...]
[2021-11-05T13:52:23,448][INFO ][o.e.h.n.Netty4HttpServerTransport] [Aj2lFRl] publish_address {127.0.0.1:9200}, bound_addresses {127.0.0.1:9200}
[2021-11-05T13:52:23,449][INFO ][o.e.n.Node               ] [Aj2lFRl] started
[2021-11-05T13:52:23,728][INFO ][o.e.g.GatewayService     ] [Aj2lFRl] recovered [1] indices into cluster_state
[2021-11-05T13:52:24,100][INFO ][o.e.c.r.a.AllocationService] [Aj2lFRl] Cluster health status changed from [RED] to [GREEN] (reason: [shards started [[.kibana_1][0]] ...]).
```

Open a web browser and targets http://localhost:9200/. Similar json must be returned.

```json
{
  "name" : "Aj2lFRl",
  "cluster_name" : "elasticsearch",
  "cluster_uuid" : "RqGRp20aRlO2b7DTkPjmDg",
  "version" : {
    "number" : "6.8.20",
    "build_flavor" : "oss",
    "build_type" : "tar",
    "build_hash" : "c859302",
    "build_date" : "2021-10-07T22:00:24.085009Z",
    "build_snapshot" : false,
    "lucene_version" : "7.7.3",
    "minimum_wire_compatibility_version" : "5.6.0",
    "minimum_index_compatibility_version" : "5.0.0"
  },
  "tagline" : "You Know, for Search"
}
```

### 2. Start K2bridge 

Start K2Bridge with appropriate settings (as explained in *K2Bridge - Application Settings* section)

Open a web browser and targets http://localhost:8080/. Similar json must be returned.

```json
{
  "name" : "Aj2lFRl",
  "cluster_name" : "elasticsearch",
  "cluster_uuid" : "RqGRp20aRlO2b7DTkPjmDg",
  "version" : {
    "number" : "6.8.20",
    "build_flavor" : "oss",
    "build_type" : "tar",
    "build_hash" : "c859302",
    "build_date" : "2021-10-07T22:00:24.085009Z",
    "build_snapshot" : false,
    "lucene_version" : "7.7.3",
    "minimum_wire_compatibility_version" : "5.6.0",
    "minimum_index_compatibility_version" : "5.0.0"
  },
  "tagline" : "You Know, for Search"
}
```

### 3. Start Kibana

Open a new terminal (don't close previous one, it will kill elastic search). 

Under Kibana's '*config*' directory, edit the *kibana.yml* file and add the following line. Note that this assumes that the bridge will listen on port 8080.

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ cd kibana-6.8.20-linux-x86_64/
vscode ➜ ~/kibana-6.8.20-linux-x86_64 $ code ./config/kibana.yml
``
```yaml
# The URLs of the Elasticsearch instances to use for all your queries.
elasticsearch.hosts: ["http://localhost:8080"]
```

Then start Kibana.
```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-6.8.20  kibana-6.8.20-linux-x86_64
vscode ➜ ~ $ cd kibana-6.8.20-linux-x86_64/
vscode ➜ ~/kibana-6.8.20-linux-x86_64 $ ./bin/kibana
  log   [14:03:45.726] [info][status][plugin:kibana@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:45.773] [info][status][plugin:elasticsearch@6.8.20] Status changed from uninitialized to yellow - Waiting for Elasticsearch
  log   [14:03:45.780] [info][status][plugin:console@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:45.797] [info][status][plugin:interpreter@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:45.813] [info][status][plugin:metrics@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:45.832] [info][status][plugin:tile_map@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:45.977] [info][status][plugin:timelion@6.8.20] Status changed from uninitialized to green - Ready
  log   [14:03:46.042] [info][status][plugin:elasticsearch@6.8.20] Status changed from yellow to green - Ready
  log   [14:03:46.159] [info][listening] Server running at http://localhost:5601
```

Open a web browser and targets http://localhost:5601/ to access the Kibana UI.

## Run end-to-end tests

Open a new terminal, and run end-to-end tests. In addition to perform tests, it will populate Azure Data Explorer with samples data.
```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ dotnet test K2Bridge.Tests.End2End/K2Bridge.Tests.End2End.csproj
```



