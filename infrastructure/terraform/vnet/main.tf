# Create virtual network
resource "azurerm_virtual_network" "main" {
  name                = var.vnet_name
  address_space       = ["10.100.0.0/16"]
  location            = var.location
  resource_group_name = var.resource_group_name
}

# Create subnet
resource "azurerm_subnet" "aks" {
  name                 = "aks-subnet"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefix       = "10.100.1.0/24"
  # Avoid update after AKS sets up routing
  # https://github.com/terraform-providers/terraform-provider-azurerm/issues/3749#issuecomment-532849895
  lifecycle {
    ignore_changes = [route_table_id]
  }
}
