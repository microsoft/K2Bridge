# Installation

## Requirements

* Helm 3
* Docker (or Azure CLI if building remotely on Azure Container Registry)
* An Azure Data Explorer instance
* An Azure AD service principal authorized to view data in Kusto

## Build the Docker container

Define arguments:

```sh
CONTAINER_NAME=<CONTAINER_NAME>
REGISTRY_NAME=<YOUR_AZURE_CONTAINER_REGISTRY_NAME>
REPOSITORY_NAME=$REGISTRY_NAME.azurecr.io

# if pulling from private ACR
IMAGE_PULL_SECRET_NAME=<YOUR ACR PULL SECRET NAME>
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

## Run on Azure Kubernetes Service (AKS)

1. Ensure your AKS instance can [pull images from ACR](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration).

    * Option A: Azure CLI (an owner role on the ACR is required):

        ```sh
        az aks update -n myAKSCluster -g myResourceGroup --attach-acr $REGISTRY_NAME
        ```

    * Option B: Kubernetes Secrets

        ```sh
        kubectl create secret docker-registry $IMAGE_PULL_SECRET_NAME --docker-server <acrname>.azurecr.io --docker-email <email> --docker-username <client id> --docker-password <client password>
        ```

1. Download the required Helm charts

    * If the k2 chart is fetched from acr:

        ```sh
        az acr helm repo add -n "<acr name>"
        ```

    * And then the Elasticsearch dependency (and k2 chart):

        ```sh
        helm repo add elastic https://helm.elastic.co
        helm repo update
        helm dependency update charts/k2bridge
        ```

1. Deploy

    * Set a few variables with the correct values for your environment:

        ```sh
        ADX_INSTANCE=[YOUR_ADX_INSTANCE_NAME]
        ADX_DATABASE=[YOUR_ADX_DATABASE_NAME]
        ADX_CLIENT_ID=[SERVICE_PRINCIPAL_CLIENT_ID]
        ADX_CLIENT_SECRET=[SERVICE_PRINCIPAL_CLIENT_SECRET]
        ADX_TENANT_ID=[SERVICE_PRINCIPAL_TENANT_ID]
        REGION=[ADX region]
        ```

    * Option A: using a local chart

        ```sh
        helm install k2bridge charts/k2bridge --set image.repository=$REPOSITORY_NAME/$CONTAINER_NAME --set settings.adxClusterUrl="https://$ADX_INSTANCE.$REGION.kusto.windows.net" --set settings.adxDefaultDatabaseName="$ADX_DATABASE" --set settings.aadClientId="$ADX_CLIENT_ID" --set settings.aadClientSecret="$ADX_CLIENT_SECRET" --set settings.aadTenantId="$ADX_TENANT_ID" --set replicaCount=2 [--set image.tag=latest] [--set privateRegistry="$IMAGE_PULL_SECRET_NAME"]
        ```

    * Option B: using a remote chart

        ```sh
        helm install k2bridge $REGISTRY_NAME/k2bridge --set image.repository=$REPOSITORY_NAME/$CONTAINER_NAME --set settings.adxClusterUrl="https://$ADX_INSTANCE.$REGION.kusto.windows.net" --set settings.adxDefaultDatabaseName="$ADX_DATABASE" --set settings.aadClientId="$ADX_CLIENT_ID" --set settings.aadClientSecret="$ADX_CLIENT_SECRET" --set settings.aadTenantId="$ADX_TENANT_ID" --set replicaCount=2 [--set image.tag=latest] [--set privateRegistry="$IMAGE_PULL_SECRET_NAME"]
        ```

    * Deploy Kibana
    The command output will suggest a helm command to run to deploy Kibana, similar to:

        ```sh
        helm install kibana elastic/kibana --set image=docker.elastic.co/kibana/kibana-oss --set imageTag=6.8.5 --set elasticsearchHosts=http://k2bridge:8080
        ```

1. Configure index-patterns
In a new installation of Kibana, you will need to configure the indexe-patterns to access your data.
Navigate to Management -> Index Patterns and create new indexes.
Note that the name of the index must be an **exact match** to the table name without any asterisk. You can copy the relevant line from the list.

Notes:
To run on other kubernetes providers, change in `values.yaml` the elasticsearch storageClassName to fit the one suggested by the provider.
