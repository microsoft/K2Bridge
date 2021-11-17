#!/bin/bash

# Display Help
Help()
{
   echo "Create data explorer cluster for development purpose."
   echo
   echo "Syntax: create-adx.sh [-u|h]"
   echo "options:"
   echo "u     Unique name used to create resources."
   echo "h     Print this Help."
   echo
}

while getopts u:h flag;
do
    case "${flag}" in
        h) Help
           exit;;
        u) unique=${OPTARG};;
    esac
done

if [ ! "$unique" ]; then
    echo "Please add unique name argument."
    exit;
fi

# Resource Names
RESOURCE_GROUP_LOCATION="westeurope"
RESOURCE_GROUP_NAME="rg-k2-$unique-dev"
ADX_CLUSTER_NAME="adxk2$unique"
ADX_DB_NAME="dev"
STORAGE_ACCOUNT_NAME="stk2$unique"
STORAGE_CONTAINER_NAME="scripts"
SERVICE_PRINCIPAL_NAME="sp-k2-$unique"

# Create Resource Group
RESOURCE_GROUP_SCOPE=$(az group create --location $RESOURCE_GROUP_LOCATION --resource-group $RESOURCE_GROUP_NAME --query id -o tsv)

# Create Data Explorer Cluster
az extension add -n kusto
echo "Please be patient. Long running operation (10 minutes)."
az kusto cluster create \
    --cluster-name $ADX_CLUSTER_NAME \
    --sku name="Dev(No SLA)_Standard_D11_v2" tier="Basic" capacity=1 \
    --resource-group $RESOURCE_GROUP_NAME \
    --location $RESOURCE_GROUP_LOCATION \
    --enable-streaming-ingest true \
    --type="None"

# Create Data Explorer Database
az kusto database create \
    --cluster-name $ADX_CLUSTER_NAME \
    --database-name $ADX_DB_NAME \
    --resource-group $RESOURCE_GROUP_NAME \
    --read-write-database soft-delete-period=P365D hot-cache-period=P31D location=$RESOURCE_GROUP_LOCATION

# Create Service Principal
SECRETS=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --role Contributor --scopes $RESOURCE_GROUP_SCOPE)
SERVICE_PRINCIPAL_FQN="aadapp=$(echo $SECRETS | jq -r '.appId');$(echo $SECRETS | jq -r '.tenant')"
SERVICE_PRINCIPAL_DISPLAYNAME=$(echo $SECRETS | jq '.displayName')

# Wait Service Principal propagation
sleep 60

# Assign Service Principal to database permissions
az kusto database add-principal \
    --cluster-name $ADX_CLUSTER_NAME \
    --database-name $ADX_DB_NAME \
    --resource-group $RESOURCE_GROUP_NAME \
    --value name=$SERVICE_PRINCIPAL_DISPLAYNAME fqn=$SERVICE_PRINCIPAL_FQN role="Viewer" type="App"

# Create Storage Account
az storage account create \
    --name $STORAGE_ACCOUNT_NAME \
    --location $RESOURCE_GROUP_LOCATION \
    --resource-group $RESOURCE_GROUP_NAME \
    --sku Standard_LRS

# Connection string used for next az storage commands  
export AZURE_STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP_NAME --query connectionString -o tsv)

# Create Storage Container
az storage container create --name $STORAGE_CONTAINER_NAME
EXPIRY=$(date -u -d '1 hour' '+%Y-%m-%dT%H:%MZ')
SAS=$(az storage container generate-sas --name $STORAGE_CONTAINER_NAME --permissions lr --expiry $EXPIRY --https-only -o tsv)

# Create Table
az storage blob upload --container-name $STORAGE_CONTAINER_NAME --file table.kql

az kusto script create \
    --cluster-name $ADX_CLUSTER_NAME \
    --database-name $ADX_DB_NAME \
    --resource-group $RESOURCE_GROUP_NAME \
    --script-url "https://$STORAGE_ACCOUNT_NAME.blob.core.windows.net/$STORAGE_CONTAINER_NAME/table.kql" \
    --script-url-sas-token "?$SAS" \
    --name "script" \
    --force-update-tag $(cat /proc/sys/kernel/random/uuid)

# Ingest sample data
DATA_FILE_NAME=data.json.gz
curl https://raw.githubusercontent.com/elastic/kibana/v6.8.5/src/server/sample_data/data_sets/flights/flights.json.gz --output $DATA_FILE_NAME

KUSTO_URL=https://$ADX_CLUSTER_NAME.$RESOURCE_GROUP_LOCATION.kusto.windows.net
TOKEN=$(az account get-access-token --scope "$KUSTO_URL/.default" -o tsv | cut -f1)

curl -X POST "$KUSTO_URL/v1/rest/ingest/$ADX_DB_NAME/kibana_data_flights?streamFormat=json" --header "Authorization: Bearer $TOKEN" --header "Content-Encoding: gzip" --header "Content-Type: multipart/form-data" --data-binary "@$DATA_FILE_NAME"
rm $DATA_FILE_NAME

# Environment variables needed to run end-to-end tests
echo "export K2BRIDGE_URL=http://localhost:8080" >> ~/.bashrc 
echo "export ELASTICSEARCH_URL=http://localhost:9200" >> ~/.bashrc 
echo "export KUSTO_URI=https://$ADX_CLUSTER_NAME.$RESOURCE_GROUP_LOCATION.kusto.windows.net" >> ~/.bashrc 
echo "export KUSTO_DB=$ADX_DB_NAME" >> ~/.bashrc 

# Variables needed to populate appsettings.development.json
echo "Use following settings/secrets in appsettings.development.json:"

echo "aadClientId: $(tput setaf 2)$(echo $SECRETS | jq -r '.appId')$(tput setaf 7)"
echo "aadClientSecret: $(tput setaf 2)$(echo $SECRETS | jq -r '.password')$(tput setaf 7)"
echo "aadTenantId: $(tput setaf 2)$(echo $SECRETS | jq -r '.tenant')$(tput setaf 7)"
echo "adxClusterUrl: $(tput setaf 2)https://$ADX_CLUSTER_NAME.$RESOURCE_GROUP_LOCATION.kusto.windows.net$(tput setaf 7)"
echo "adxDefaultDatabaseName: $(tput setaf 2)$ADX_DB_NAME$(tput setaf 7)"