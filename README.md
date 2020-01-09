# Kibana Kusto Bridge

**A bridge connecting Kibana to Kusto (Azure Data Explorer) as its backend database**

---

[![Build Status](https://dev.azure.com/csedevil/Kibana-kusto-bridge/_apis/build/status/microsoft.KibanaKustoBridge?branchName=master)](https://dev.azure.com/csedevil/Kibana-kusto-bridge/_build/latest?definitionId=140&branchName=master)
[![MIT license](https://img.shields.io/badge/license-MIT-brightgreen.svg)](http://opensource.org/licenses/MIT)

## Description

## Installation

See the Installation document

## Development

Running Kibana and KibanaKustoBridge locally for testing and development, see [here](./docs/development.md)

### Requirements

* Helm 3
* Docker (or Azure CLI if building remotely on Azure Container Registry)
* An Azure Data Explorer instance
* An Azure AD service principal authorized to view data in Kusto

### Build the Docker container

Define arguments:

```sh

CONTAINER_NAME=[CONTAINER_NAME]
REGISTRY_NAME=[YOUR_AZURE_CONTAINER_REGISTRY_NAME]
REPOSITORY_NAME=$REGISTRY_NAME.azurecr.io

# if pulling from private ACR
IMAGE_PULL_SECRET_NAME=[YOUR ACR PULL SECRET NAME]

```

You can build the container on a local Docker installation:

```sh

docker build -t $CONTAINER_NAME .
docker push $REPOSITORY_NAME/$CONTAINER_NAME
```

Or you can build the container remotely on Azure Container Registry:

```sh

az acr build -r $REGISTRY_NAME -t $CONTAINER_NAME .
```

### Run on Azure Kubernetes Service

Ensure your AKS instance can [pull images from ACR](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration).

Using option A (needs owner role on ACR):

```sh
az aks update -n myAKSCluster -g myResourceGroup --attach-acr $REGISTRY_NAME
```

OR option B:

```sh
kubectl create secret docker-registry $IMAGE_PULL_SECRET_NAME --docker-server <acrname>.azurecr.io --docker-email <email> --docker-username=<client id> --docker-password <client password>
```

Download the Elasticsearch helm chart dependency (and k2 helm chart):

If the k2 chart is fetched from acr:
```sh
az acr helm repo add -n "<acr name>"
```

Elasticsearch chart:
```sh
helm repo add elastic https://helm.elastic.co
helm repo update
helm dependency update charts/k2bridge
```

Deploy

```sh
ADX_INSTANCE=[YOUR_ADX_INSTANCE_NAME]
ADX_DATABASE=[YOUR_ADX_DATABASE_NAME]
ADX_CLIENT_ID=[SERVICE_PRINCIPAL_CLIENT_ID]
ADX_CLIENT_SECRET=[SERVICE_PRINCIPAL_CLIENT_SECRET]
ADX_TENANT_ID=[SERVICE_PRINCIPAL_TENANT_ID]
REGION=[ADX region]
```

local chart:
```sh
helm install k2bridge charts/k2bridge --set image.repository=$REPOSITORY_NAME/$CONTAINER_NAME --set settings.kustoClusterUrl="https://$ADX_INSTANCE.$REGION.kusto.windows.net" --set settings.kustoDatabase="$ADX_DATABASE" --set settings.kustoAadClientId="$ADX_CLIENT_ID" --set settings.kustoAadClientSecret="$ADX_CLIENT_SECRET" --set settings.kustoAadTenantId="$ADX_TENANT_ID" --set replicaCount=2 [-set privateRegistry="$IMAGE_PULL_SECRET_NAME"]
```

remote chart:
```sh
helm install k2bridge $REGISTRY_NAME/k2bridge --set image.repository=$REPOSITORY_NAME/$CONTAINER_NAME --set settings.kustoClusterUrl="https://$ADX_INSTANCE.$REGION.kusto.windows.net" --set settings.kustoDatabase="$ADX_DATABASE" --set settings.kustoAadClientId="$ADX_CLIENT_ID" --set settings.kustoAadClientSecret="$ADX_CLIENT_SECRET" --set settings.kustoAadTenantId="$ADX_TENANT_ID" --set replicaCount=2 [-set privateRegistry="$IMAGE_PULL_SECRET_NAME"]
```

The command output will suggest a helm command to run to deploy Kibana, similar to:

```sh
helm install kibana elastic/kibana --set image=docker.elastic.co/kibana/kibana-oss --set imageTag=6.8.5 --set elasticsearchHosts=http://k2bridge:8080
```

In a new installation of Kibana, you will need to configure the indexes. Navigate to Management -> Index Patterns and create new indexes.
Note that the name of the index must be an exact match to the table name.

Notes:
 - To run on other kubernetes providers, change in `values.yaml` the elasticsearch storageClassName to fit the one suggested by the provider.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
