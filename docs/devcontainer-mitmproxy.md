# Intercept requests using mitmproxy

If you need to intercept http requests between Kibana <---> K2Bridge, the development container provides [mitmproxy](https://mitmproxy.org/) tools. mitmproxy is a free and open source interactive HTTPS proxy.

## Steps to configure and run mitmproxy

### 1. Start Elastic Search

```bash
vscode ➜ /workspaces/K2Bridge/.devcontainer (feature/devcontainer-7.10 ✗) $ cd $home
vscode ➜ ~ $ cd elasticsearch-7.16.2/
vscode ➜ ~/elasticsearch-7.16.2 $ ./bin/elasticsearch

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
    "number" : "7.16.2",
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

### 2. Start K2Bridge

Start K2Bridge with appropriate settings (as explained in [development](./development.md) section). 
However, in appsettings.Development.json, please change the default K2Bridge listener port (for instance 8081). Do not use 8080, it will be preempted by mitmproxy.

``` json
[...]
"bridgeListenerAddress": "http://localhost:8081",
[...]
```

Open a web browser and target http://localhost:8081/. Similar json must be returned.

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

### 3. Start mitmweb 

mitmweb is a web-based interface for mitmproxy. The reverse proxy mode will intercept requests on 8080 port and redirect them to K2Bridge (running on 8081). 

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer-7.10 ✗) $ mitmweb --web-port 8888 --mode reverse:http://localhost:8081
Web server listening at http://127.0.0.1:8888/
Proxy server listening at http://*:8080
```

Open a web browser and target http://localhost:8888/ to access the mitmproxy Web UI.

### 4.Start Kibana

Under Kibana's '*config*' directory, edit the *kibana.yml* file and add the following line. Note that this assumes that mitmproxy will listen on port 8080.

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer-7.10) $ cd $home
vscode ➜ ~ $ cd kibana-7.16.2-linux-x86_64/
vscode ➜ ~/kibana-7.16.2-linux-x86_64 $ code ./config/kibana.yml
```
```yaml
# The URLs of the Elasticsearch instances to use for all your queries.
elasticsearch.hosts: ["http://localhost:8080"]
```

Then start Kibana.
```bash
vscode ➜ ~/kibana-7.16.2-linux-x86_64 $ ./bin/kibana
[...]
  log   [10:32:18.179] [info][plugins-system] Starting [40] plugins: [usageCollection,telemetryCollectionManager,telemetry,kibanaUsageCollection,securityOss,newsfeed,mapsLegacy,kibanaLegacy,share,legacyExport,embeddable,expressions,data,home,console,apmOss,management,indexPatternManagement,advancedSettings,savedObjects,dashboard,visualizations,inputControlVis,visTypeVega,visTypeTimelion,timelion,visTypeTable,visTypeMarkdown,tileMap,regionMap,visualize,esUiShared,charts,visTypeTimeseries,visTypeVislib,visTypeTagcloud,visTypeMetric,discover,savedObjectsManagement,bfetch]
  log   [10:32:18.793] [info][listening] Server running at http://localhost:5601
  log   [10:32:18.851] [info][server][Kibana][http] http server running at http://localhost:5601
```

Open a web browser and target http://localhost:5601/ to access the Kibana UI.
