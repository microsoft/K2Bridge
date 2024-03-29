#!/bin/bash

# Display Help
Help()
{
    echo "Create Azure Data Explorer (ADX) cluster for development purpose."
    echo
    echo "Syntax: create-adx.sh [-u|l|h]"
    echo "options:"
    echo "u     Unique name used to create resources."
    echo "l     Location name. Values from: az account list-locations."
    echo "h     Print this Help."
    echo
}

while getopts u:l:h flag;
do
    case "${flag}" in
        h) Help
            exit;;
        u) unique=${OPTARG};;
        l) location=${OPTARG};;
    esac
done

if [ ! "$unique" ]; then
    echo "Please add unique name argument."
    exit;
fi

if [ ! "$location" ]; then
    echo "Please add location argument."
    exit;
fi

# Resource Names
LOCATION=$location
RESOURCE_GROUP_NAME="rg-k2-$unique-dev"
ADX_CLUSTER_NAME="adxk2$unique"
ADX_DB_NAME="dev"
STORAGE_ACCOUNT_NAME="stk2$unique"
STORAGE_CONTAINER_NAME="scripts"
SERVICE_PRINCIPAL_NAME="sp-k2-$unique"

# Colors
COLOR_GREEN=$(tput setaf 2)
COLOR_YELLOW=$(tput setaf 3)
COLOR_WHITE=$(tput setaf 7)

# Create Resource Group
RESOURCE_GROUP_SCOPE=$(az group create --location $LOCATION --resource-group $RESOURCE_GROUP_NAME --query id -o tsv)

# Create Azure Data Explorer Cluster
az extension add -n kusto
echo "${COLOR_GREEN}[1/8] Creating Azure Data Explorer Cluster. Please be patient. Long running operation (10 minutes).${COLOR_WHITE}"
az kusto cluster create \
    --cluster-name $ADX_CLUSTER_NAME \
    --sku name="Dev(No SLA)_Standard_D11_v2" tier="Basic" capacity=1 \
    --resource-group $RESOURCE_GROUP_NAME \
    --location $LOCATION \
    --enable-streaming-ingest true \
    --type="None" # Argument added to fix issue https://github.com/Azure/azure-cli/issues/18827

# Create Azure Data Explorer Database
echo "${COLOR_GREEN}[2/8] Creating Azure Data Explorer Database.${COLOR_WHITE}"
az kusto database create \
    --cluster-name $ADX_CLUSTER_NAME \
    --database-name $ADX_DB_NAME \
    --resource-group $RESOURCE_GROUP_NAME \
    --read-write-database soft-delete-period=P365D hot-cache-period=P31D location=$LOCATION

# Create Service Principal
echo "${COLOR_GREEN}[3/8] Creating Service Principal.${COLOR_WHITE}"
SECRETS=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --role Contributor --scopes $RESOURCE_GROUP_SCOPE)
SERVICE_PRINCIPAL_FQN="aadapp=$(echo $SECRETS | jq -r '.appId');$(echo $SECRETS | jq -r '.tenant')"
SERVICE_PRINCIPAL_DISPLAYNAME=$(echo $SECRETS | jq '.displayName')

# Wait Service Principal propagation
echo "${COLOR_GREEN}[4/8] Wait 60 seconds until Service Principal is propagated.${COLOR_WHITE}"
sleep 60

# Assign Service Principal to database permissions
echo "${COLOR_GREEN}[5/8] Assigning database permissions.${COLOR_WHITE}"
az kusto database add-principal \
    --cluster-name $ADX_CLUSTER_NAME \
    --database-name $ADX_DB_NAME \
    --resource-group $RESOURCE_GROUP_NAME \
    --value name=$SERVICE_PRINCIPAL_DISPLAYNAME fqn=$SERVICE_PRINCIPAL_FQN role="Viewer" type="App"

# Create Storage Account
echo "${COLOR_GREEN}[6/8] Creating Storage Account.${COLOR_WHITE}"
az storage account create \
    --name $STORAGE_ACCOUNT_NAME \
    --location $LOCATION \
    --resource-group $RESOURCE_GROUP_NAME \
    --sku Standard_LRS

# Connection string used for next az storage commands
export AZURE_STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP_NAME --query connectionString -o tsv)

# Create Storage Container
az storage container create --name $STORAGE_CONTAINER_NAME
EXPIRY=$(date -u -d '1 hour' '+%Y-%m-%dT%H:%MZ')
SAS=$(az storage container generate-sas --name $STORAGE_CONTAINER_NAME --permissions lr --expiry $EXPIRY --https-only -o tsv)

# Create Azure Data Explorer Table
echo "${COLOR_GREEN}[7/8] Creating Azure Data Explorer Table.${COLOR_WHITE}"
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
echo "${COLOR_GREEN}[8/8] Ingesting Sample Data.${COLOR_WHITE}"
DATA_FILE_NAME=data.json.gz
curl https://raw.githubusercontent.com/elastic/kibana/v6.8.5/src/server/sample_data/data_sets/flights/flights.json.gz --output $DATA_FILE_NAME

KUSTO_URL=https://$ADX_CLUSTER_NAME.$LOCATION.kusto.windows.net
TOKEN=$(az account get-access-token --scope "$KUSTO_URL/.default" --query accessToken -o tsv)

curl -X POST "$KUSTO_URL/v1/rest/ingest/$ADX_DB_NAME/kibana_data_flights?streamFormat=json" --header "Authorization: Bearer $TOKEN" --header "Content-Encoding: gzip" --header "Content-Type: multipart/form-data" --data-binary "@$DATA_FILE_NAME"
rm $DATA_FILE_NAME

# Environment variables needed to run end-to-end tests
echo "export K2BRIDGE_URL=http://localhost:8080" >> ~/.bashrc
echo "export ELASTICSEARCH_URL=http://localhost:9200" >> ~/.bashrc
echo "export KUSTO_URI=https://$ADX_CLUSTER_NAME.$LOCATION.kusto.windows.net" >> ~/.bashrc
echo "export KUSTO_DB=$ADX_DB_NAME" >> ~/.bashrc

# Reminder
echo -e "\n"
echo "${COLOR_YELLOW}At the end of your development, don't forget to delete service principal and its role assignements."
echo "${COLOR_YELLOW}az ad sp delete --id $(echo $SECRETS | jq -r '.appId')"

# Variables needed to populate appsettings.development.json
echo -e "\n"
echo "${COLOR_GREEN}Use following settings/secrets in appsettings.development.json:${COLOR_WHITE}"

echo "aadClientId: ${COLOR_GREEN}$(echo $SECRETS | jq -r '.appId')${COLOR_WHITE}"
echo "aadClientSecret: ${COLOR_GREEN}$(echo $SECRETS | jq -r '.password')${COLOR_WHITE}"
echo "aadTenantId: ${COLOR_GREEN}$(echo $SECRETS | jq -r '.tenant')${COLOR_WHITE}"
echo "adxClusterUrl: ${COLOR_GREEN}https://$ADX_CLUSTER_NAME.$LOCATION.kusto.windows.net${COLOR_WHITE}"
echo "adxDefaultDatabaseName: ${COLOR_GREEN}$ADX_DB_NAME${COLOR_WHITE}"
