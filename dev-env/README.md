# How to setup an Elasticsearch environment for development / internal testing

1. Run the following script to install both Elasticsearch and Kibana (be sure to set the namespace variable).
Note: the version set in the script can be changed but settings might need to be adjusted.

    ```bash
    $NAMESPACE='my-elastic-search'
    $VERSION='6.8.5'

    helm repo add elastic https://helm.elastic.co

    # create a new namespace
    kubectl create namespace $NAMESPACE

    # install elastic search
    helm install elasticsearch elastic/elasticsearch --namespace $NAMESPACE \
        --set clusterName="elasticsearch" \
        --set image="docker.elastic.co/elasticsearch/elasticsearch-oss" \
        --set imageTag="$VERSION" \
        --set replicas=1 \
        --set minimumMasterNodes=1 \
        --set antiAffinity="soft" \
        --set esJavaOpts="-Xmx128m -Xms128m" \
        --set resources.requests.cpu="100m" \
        --set resources.requests.memory="512M" \
        --set limits.cpu="1000m" \
        --set limits.memory="512M" \
        --set volumeClaimTemplate.resources.requests.storage="1Gi" \
        --set service.type="LoadBalancer"

    # install Kibana
    helm install kibana elastic/kibana --namespace $NAMESPACE
        --set elasticsearchHosts="http://elasticsearch-master:9200" \
        --set image="docker.elastic.co/kibana/kibana-oss" \
        --set imageTag="$VERSION" \
        --set persistentVolumeClaim.size="1Gi" \
        --set service.type="LoadBalancer"
    ```

1. Import Data
    We want to be able to compare the results we get between Elasticsearch and ADX (Kusto). To do so we need the exact same data available in both datastores.

    Kibana offers sample data and an easy way to import it to Elasticsearch. The caveat is that while doing so, it also shifts all the timestamps to start near the import time (and last over a period of a few weeks to the future).
    After doing that in Kibana you'll need to export data from Elasticsearch and import it to Kusto.
    [elastidump](https://github.com/taskrabbit/elasticsearch-dump) is a utility able to import/export Elasticsearch data (as well as mappings and other objects). You should use that to export the data from Elasticsearch and then import it to Kusto

    You can see example Kusto commands [here](tables.kql). Those commands include tables creation and how to convert an item-object to discreet fields.
