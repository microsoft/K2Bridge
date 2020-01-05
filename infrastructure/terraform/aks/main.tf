# Application Insights

resource "random_id" "workspace" {
  keepers = {
    # Generate a new id each time we switch to a new resource group
    group_name = var.resource_group_name
  }

  byte_length = 8
}

resource "azurerm_log_analytics_workspace" "aks" {
  name                = "k8s-workspace-${random_id.workspace.hex}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
}

resource "azurerm_log_analytics_solution" "aks" {
  solution_name         = "ContainerInsights"
  location              = var.location
  resource_group_name   = var.resource_group_name
  workspace_resource_id = azurerm_log_analytics_workspace.aks.id
  workspace_name        = azurerm_log_analytics_workspace.aks.name

  plan {
    publisher = "Microsoft"
    product   = "OMSGallery/ContainerInsights"
  }
}

# Subnet permission

resource "azurerm_role_assignment" "aks_subnet" {
  scope                = var.subnet_id
  role_definition_name = "Network Contributor"
  principal_id         = var.aks_sp_object_id
}

# Kubernetes Service

resource "azurerm_kubernetes_cluster" "aks" {
  name                = var.aks_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = var.aks_name
  kubernetes_version  = var.aks_version

  default_node_pool {
    name            = "default"
    node_count      = 6
    vm_size         = "Standard_D2s_v3"
    os_disk_size_gb = 30
    vnet_subnet_id  = var.subnet_id
  }

  addon_profile {
   oms_agent {
     enabled                    = true
     log_analytics_workspace_id = azurerm_log_analytics_workspace.aks.id
    }
  }

  service_principal {
    client_id     = var.aks_sp_client_id
    client_secret = var.aks_sp_client_secret
  }

  depends_on = [
    azurerm_role_assignment.aks_subnet
  ]
}
