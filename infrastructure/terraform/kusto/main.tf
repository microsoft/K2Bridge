resource "azurerm_kusto_cluster" "kusto" {
  name                = var.kusto_name
  location            = var.location
  resource_group_name = var.resource_group_name

  sku {
    name     = "Dev(No SLA)_Standard_D11_v2"
    capacity = 1
  }
}

resource "azurerm_role_assignment" "aks_kusto" {
  scope                = azurerm_kusto_cluster.kusto.id
  role_definition_name = "Contributor"
  principal_id         = var.kusto_admin_sp_object_id
}
