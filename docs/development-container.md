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
| elasticsearch-oss| 6.8.22 |
| kibana-oss | 6.8.22 |

Elasticsearch and Kibana are available at `/home/vscode`; and dev container will start using vscode non-root user. For reference, you cannot start elasticsearch using root user, an exception will occur.

## Quick Steps to launch Elastic Stack

### 1. Start Elastic Search

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-6.8.22  kibana-6.8.22-linux-x86_64
vscode ➜ ~ $ cd elasticsearch-6.8.22/
vscode ➜ ~/elasticsearch-6.8.22 $ ./bin/elasticsearch

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
    "number" : "6.8.22",
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

### 2. Start Kibana

Open a new terminal (don't close previous one, it will kill elastic search). 

Then start Kibana.
```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ ls
elasticsearch-6.8.22  kibana-6.8.22-linux-x86_64
vscode ➜ ~ $ cd kibana-6.8.22-linux-x86_64/
vscode ➜ ~/kibana-6.8.22-linux-x86_64 $ ./bin/kibana
  log   [14:03:45.726] [info][status][plugin:kibana@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:45.773] [info][status][plugin:elasticsearch@6.8.22] Status changed from uninitialized to yellow - Waiting for Elasticsearch
  log   [14:03:45.780] [info][status][plugin:console@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:45.797] [info][status][plugin:interpreter@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:45.813] [info][status][plugin:metrics@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:45.832] [info][status][plugin:tile_map@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:45.977] [info][status][plugin:timelion@6.8.22] Status changed from uninitialized to green - Ready
  log   [14:03:46.042] [info][status][plugin:elasticsearch@6.8.22] Status changed from yellow to green - Ready
  log   [14:03:46.159] [info][listening] Server running at http://localhost:5601
```

Open a web browser and targets http://localhost:5601/ to access the Kibana UI.

### Important

To use the bridge, you need to update kibana config file. 

Under Kibana's '*config*' directory, edit the *kibana.yml* file.

```bash
vscode ➜ /workspaces/K2Bridge (feature/devcontainer ✗) $ cd $home
vscode ➜ ~ $ cd kibana-6.8.22-linux-x86_64/
vscode ➜ ~/kibana-6.8.22-linux-x86_64 $ code ./config/kibana.yml
```

And add the following line. Note that this assumes that the bridge will listen on port 8080. All requests will be forwarded to bridge. 

```yaml
# The URLs of the Elasticsearch instances to use for all your queries.
elasticsearch.hosts: ["http://localhost:8080"]
```

## What's next !?

Check the [development](./development.md) instructions to configure correctly K2Bridge application settings. 


