# Deploy a Resource Group with an Azure ML Workspace and supporting resources.
#
# For suggested naming conventions, refer to:
#   https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging

# Resource Group

module "vnet" {
  source = "./vnet"
  vnet_name = var.vnet_name
  resource_group_name = var.resource_group
  location = var.location
}

module "aks" {
  source = "./aks"
  aks_name = var.aks_name
  aks_version = var.aks_version
  resource_group_name = var.resource_group
  location = var.location
  subnet_id = module.vnet.aks_subnet_id
  aks_sp_client_id = var.aks_sp_client_id
  aks_sp_object_id = var.aks_sp_object_id
  aks_sp_client_secret = var.aks_sp_client_secret
}

module "kusto" {
  source = "./kusto"
  kusto_name = var.kusto_name
  resource_group_name = var.resource_group
  location = var.location
  kusto_admin_sp_object_id = var.kusto_admin_sp_object_id
}
