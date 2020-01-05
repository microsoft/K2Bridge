output "subscription_id" {
  value = data.azurerm_client_config.current.subscription_id
}

output "vnet_id" {
  value = module.vnet.id
}

output "aks_id" {
  value = module.aks.id
}

output "kube_config" {
  value     = module.aks.kube_config
  sensitive = true
}

output "kubernetes_version" {
  value = module.aks.kubernetes_version
}
