# Development container for Visual Studio Code

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
| mitmproxy | 7.0.4 |
| dotnet-sdk-3.1 | latest |
| dotnet-sdk-5.0 | latest |
| default-jre | latest |
| elasticsearch-oss| 7.10.2 |
| kibana-oss | 7.10.2 |

Elasticsearch and Kibana are available at `/home/vscode`; and dev container will start using vscode non-root user. For reference, you cannot start elasticsearch using root user, an exception will occur.

## Quick Steps to launch Elastic Stack

### 1. Start Elastic Search

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-7.10.2  kibana-7.10.2-linux-x86_64
vscode ➜ ~ $ cd elasticsearch-7.10.2/
vscode ➜ ~/elasticsearch-7.10.2 $ ./bin/elasticsearch

[...]
[2021-11-10T10:29:41,395][INFO ][o.e.h.AbstractHttpServerTransport] [9f911f701eba] publish_address {127.0.0.1:9200}, bound_addresses {127.0.0.1:9200}
[2021-11-10T10:29:41,397][INFO ][o.e.n.Node               ] [9f911f701eba] started
[2021-11-10T10:29:41,444][INFO ][o.e.g.GatewayService     ] [9f911f701eba] recovered [0] indices into cluster_state
```

Open a web browser and target http://localhost:9200/. Similar json must be returned.

```json
{
  "name" : "9f911f701eba",
  "cluster_name" : "elasticsearch",
  "cluster_uuid" : "IfhhAPkOT8m_zfIkUfHtWg",
  "version" : {
    "number" : "7.10.2",
    "build_flavor" : "oss",
    "build_type" : "tar",
    "build_hash" : "747e1cc71def077253878a59143c1f785afa92b9",
    "build_date" : "2021-01-13T00:42:12.435326Z",
    "build_snapshot" : false,
    "lucene_version" : "8.7.0",
    "minimum_wire_compatibility_version" : "6.8.0",
    "minimum_index_compatibility_version" : "6.0.0-beta1"
  },
  "tagline" : "You Know, for Search"
}
```

### 2. Start Kibana

Open a new terminal (don't close previous one, it will kill elastic search). 

Then start Kibana.
```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-7.10.2  kibana-7.10.2-linux-x86_64
vscode ➜ ~ $ cd kibana-7.10.2-linux-x86_64/
vscode ➜ ~/kibana-7.10.2-linux-x86_64 $ ./bin/kibana
  log   [10:32:16.542] [info][plugins-service] Plugin "visTypeXy" is disabled.
  log   [10:32:16.711] [info][plugins-system] Setting up [40] plugins: [usageCollection,telemetryCollectionManager,telemetry,kibanaUsageCollection,securityOss,newsfeed,mapsLegacy,kibanaLegacy,share,legacyExport,embeddable,expressions,data,home,console,apmOss,management,indexPatternManagement,advancedSettings,savedObjects,dashboard,visualizations,inputControlVis,visTypeVega,visTypeTimelion,timelion,visTypeTable,visTypeMarkdown,tileMap,regionMap,visualize,esUiShared,charts,visTypeTimeseries,visTypeVislib,visTypeTagcloud,visTypeMetric,discover,savedObjectsManagement,bfetch]
  log   [10:32:17.005] [info][savedobjects-service] Waiting until all Elasticsearch nodes are compatible with Kibana before starting saved objects migrations...
  log   [10:32:17.061] [info][savedobjects-service] Starting saved objects migrations
  log   [10:32:17.101] [info][savedobjects-service] Creating index .kibana_1.
  log   [10:32:18.047] [info][savedobjects-service] Pointing alias .kibana to .kibana_1.
  log   [10:32:18.162] [info][savedobjects-service] Finished in 1071ms.
  log   [10:32:18.179] [info][plugins-system] Starting [40] plugins: [usageCollection,telemetryCollectionManager,telemetry,kibanaUsageCollection,securityOss,newsfeed,mapsLegacy,kibanaLegacy,share,legacyExport,embeddable,expressions,data,home,console,apmOss,management,indexPatternManagement,advancedSettings,savedObjects,dashboard,visualizations,inputControlVis,visTypeVega,visTypeTimelion,timelion,visTypeTable,visTypeMarkdown,tileMap,regionMap,visualize,esUiShared,charts,visTypeTimeseries,visTypeVislib,visTypeTagcloud,visTypeMetric,discover,savedObjectsManagement,bfetch]
  log   [10:32:18.793] [info][listening] Server running at http://localhost:5601
  log   [10:32:18.851] [info][server][Kibana][http] http server running at http://localhost:5601
```

Open a web browser and target http://localhost:5601/ to access the Kibana UI.

### Important

To use the bridge, you need to update kibana config file. 

Under Kibana's '*config*' directory, edit the *kibana.yml* file.

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ cd kibana-7.10.2-linux-x86_64/
vscode ➜ ~/kibana-7.10.2-linux-x86_64 $ code ./config/kibana.yml 
```

And add the following line. Note that this assumes that the bridge will listen on port 8080. All requests will be forwarded to bridge. 

```yaml
# The URLs of the Elasticsearch instances to use for all your queries.
elasticsearch.hosts: ["http://localhost:8080"]
```

## What's next !?

Check the [development](./development.md) instructions to configure correctly K2Bridge application settings.