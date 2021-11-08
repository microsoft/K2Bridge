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

# Main Properties
RESOURCE_GROUP_LOCATION="westeurope"
RESOURCE_GROUP_NAME="rg-k2-$unique-dev"

ADX_CLUSTER_NAME="adxk2$unique"
ADX_DB_NAME="devdatabase"

SERVICE_PRINCIPAL_NAME="sp-k2-$unique"

# Create Resource Group
RESOURCE_GROUP_SCOPE=$(az group create --location $RESOURCE_GROUP_LOCATION --resource-group $RESOURCE_GROUP_NAME --query id -o tsv)

# Create Data Explorer Cluster
az extension add -n kusto
az kusto cluster create --cluster-name $ADX_CLUSTER_NAME --sku name="Dev(No SLA)_Standard_D11_v2" tier="Basic" capacity=1 --resource-group $RESOURCE_GROUP_NAME --location $RESOURCE_GROUP_LOCATION --type="None" 

kusto_scope=$(az kusto cluster show --cluster-name $ADX_CLUSTER_NAME --resource-group $RESOURCE_GROUP_NAME --query id -o tsv)
echo $kusto_scope

# Create Data Explorer Database
az kusto database create --cluster-name $ADX_CLUSTER_NAME --database-name $ADX_DB_NAME --resource-group $RESOURCE_GROUP_NAME --read-write-database soft-delete-period=P365D hot-cache-period=P31D location=$RESOURCE_GROUP_LOCATION

# Create Service Principal
secrets=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --role Contributor --scopes $RESOURCE_GROUP_SCOPE)
SERVICE_PRINCIPAL_FQN="aadapp=$(echo $secrets | jq -r '.appId');$(echo $secrets | jq -r '.tenant')"
SERVICE_PRINCIPAL_DISPLAYNAME=$(echo $secrets | jq '.displayName')

# Assign Service Principal to database permissions
sleep 60
az kusto database add-principal --cluster-name $ADX_CLUSTER_NAME --database-name $ADX_DB_NAME --resource-group $RESOURCE_GROUP_NAME --value name=$SERVICE_PRINCIPAL_DISPLAYNAME fqn=$SERVICE_PRINCIPAL_FQN role="Viewer" type="App"

# Environment variables needed to run end-to-end tests
echo "export K2BRIDGE_URL=http://localhost:8080" >> ~/.bashrc 
echo "export ELASTICSEARCH_URL=http://localhost:9200" >> ~/.bashrc 
echo "export KUSTO_URI=https://$ADX_CLUSTER_NAME.$RESOURCE_GROUP_LOCATION.kusto.windows.net" >> ~/.bashrc 
echo "export KUSTO_DB=$ADX_DB_NAME" >> ~/.bashrc 

# Variables needed to populate appsettings.development.json
echo "Use following settings/secrets in appsettings.development.json:"

echo "aadClientId: $(tput setaf 2)$(echo $secrets | jq -r '.appId')$(tput setaf 7)"
echo "aadClientSecret: $(tput setaf 2)$(echo $secrets | jq -r '.password')$(tput setaf 7)"
echo "aadTenantId: $(tput setaf 2)$(echo $secrets | jq -r '.tenant')$(tput setaf 7)"
echo "adxClusterUrl: $(tput setaf 2)https://$ADX_CLUSTER_NAME.$RESOURCE_GROUP_LOCATION.kusto.windows.net$(tput setaf 7)"
echo "adxDefaultDatabaseName: $(tput setaf 2)$ADX_DB_NAME$(tput setaf 7)"
