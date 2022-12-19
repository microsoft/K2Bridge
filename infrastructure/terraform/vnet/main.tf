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
  address_prefixes       = ["10.100.1.0/24"]
}
