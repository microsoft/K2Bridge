variable "environment" {
  type    = string
  description = "Environment name, e.g. 'dev' or 'stage'"
  default = "dev"
}

variable "resource_group" {
  type    = string
  description = "Resource group to deploy in."
}

variable "location" {
  type    = string
  description = "Azure region where to create resources."
  default = "West Europe"
}

variable "vnet_name" {
  type = string
  description = "Name of the generated VNET."
}

variable "aks_name" {
  type = string
  description = "Name of the generated Azure Kubernetes Service cluster."
}

variable "aks_version" {
  type = string
  description = "Kubernetes version of the AKS cluster."
}

variable "aks_sp_client_id" {
  type = string
  description = "Service principal client ID for the Azure Kubernetes Service cluster identity."
}

variable "aks_sp_object_id" {
  type = string
  description = "Service principal object ID for the Azure Kubernetes Service cluster identity. Should be object IDs of service principals, not object IDs of the application nor application IDs. To retrieve, navigate in the AAD portal from an App registration to 'Managed application in local directory'."
}

variable "aks_sp_client_secret" {
  type = string
  description = "Service principal client secret for the Azure Kubernetes Service cluster identity."
}

variable "kusto_name" {
  type = string
  description = "Name of the generated Kusto cluster."
}

variable "kusto_admin_sp_object_id" {
  type = string
  description = "Service principal object ID for the principal to be granted Contributor permissions on the Kusto cluster."
}
