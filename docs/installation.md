# Installation

## Requirements

* AKS cluster ([create an AKS cluster instructions](https://docs.microsoft.com/en-us/azure/aks/kubernetes-walkthrough-portal#create-an-aks-cluster)) or any other Kubernetes cluster (version 1.14 or newer - tested and verified up to version 1.16. A minimum of 3 node count).
You need to be able to connect to your cluster from your machine.
* [Helm 3](https://github.com/helm/helm#install)
* An Azure Data Explorer (ADX) instance
    * You will need the ADX cluster URL and a database name
* An Azure AD service principal authorized to view data in ADX
    * You will need the ClientID and Secret
    * [Create an Azure AD service principal instructions](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application)
    * Instructions on how to set cluster's view permissions for the Azure AD service principal, can be found here: [manage permissions](https://docs.microsoft.com/en-us/azure/data-explorer/manage-database-permissions#manage-permissions-in-the-azure-portal)
    * Important note: A service principal with 'Viewer' permission is enough. It is highly discouraged to use higher permissions (read, admin, etc...)
* [Optional] - Docker for image build
    If you need to build the image, please follow the [build instructions](./build.md).

## Run on Azure Kubernetes Service (AKS)

1. By default, K2's Helm chart references a publicly available image located on Microsoft's Container Registry (MCR) which does not require any credentials and work out of the box.

1. Download the required Helm charts

    1. Add the Elasticsearch dependency to Helm:

        ```sh
        helm repo add elastic https://helm.elastic.co
        helm repo update
        ```

    1. The K2 chart is located under the [charts](../charts) directory. Clone the repository to get the chart.

        1. go to the K2 root repository directory

        1. run

            ```sh
            helm dependency update charts/k2bridge
            ```

1. Deploy

    * Recommended - create a kubernetis namespace.  
         This namespace is later passed to `helm install` via the `-n` switch.

      ```sh
      kubectl create namespace k2bridge
      ```

    * Set a few variables with the correct values for your environment:

        ```sh
        ADX_URL=[YOUR_ADX_CLUSTER_URL e.g. https://mycluster.westeurope.kusto.windows.net]
        ADX_DATABASE=[YOUR_ADX_DATABASE_NAME]
        ADX_CLIENT_ID=[SERVICE_PRINCIPAL_CLIENT_ID]
        ADX_CLIENT_SECRET=[SERVICE_PRINCIPAL_CLIENT_SECRET]
        ADX_TENANT_ID=[SERVICE_PRINCIPAL_TENANT_ID]
        ```

        Optional - enable ApplicationInsights telemetry

        ```sh
        APPLICATION_INSIGHTS_KEY=[INSTRUMENTATION_KEY]
        COLLECT_TELEMETRY=true
        ```

        ```sh
        helm install k2bridge charts/k2bridge -n k2bridge --set settings.adxClusterUrl="$ADX_URL" --set settings.adxDefaultDatabaseName="$ADX_DATABASE" --set settings.aadClientId="$ADX_CLIENT_ID" --set settings.aadClientSecret="$ADX_CLIENT_SECRET" --set settings.aadTenantId="$ADX_TENANT_ID" [--set image.tag=latest] [--set settings.collectTelemetry=$COLLECT_TELEMETRY]
        ```

        The complete set of configuration options can be found [here](./configuration.md).

    * Deploy Kibana  
        The command output will suggest a helm command to run to deploy Kibana, similar to:

        ```sh
        helm install kibana elastic/kibana -n k2bridge --set image=docker.elastic.co/kibana/kibana-oss --set imageTag=7.10.2 --set elasticsearchHosts=http://k2bridge:8080
        ```

1. Configure index-patterns  
In a new installation of Kibana, you will need to configure the index-patterns to access your data.
Navigate to Management -> Index Patterns -> Create index pattern.
Note that the name of the index must be an **exact match** to the table name or function name, without any asterisk. You can copy the relevant line from the list.

Notes:
To run on other kubernetes providers, change in `values.yaml` the elasticsearch storageClassName to fit the one suggested by the provider.
