#Set the terraform required version

terraform {
  required_version = ">= 0.12.6"
}

# Configure the Azure Provider

provider "azurerm" {
  # It is recommended to pin to a given version of the Provider
  version = "=1.38.0"
}

# Configure other Providers

provider "random" {
  version = "~> 2.2"
}

# Data

# Provides client_id, tenant_id, subscription_id and object_id variables
data "azurerm_client_config" "current" {}

