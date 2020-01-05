# Terraform templates

In the Azure DevOps pipeline, the `terraform_backend` directory is merged into the `terraform` directory
before running apply, so that state is maintained in Azure Storage. Deployed resources are named with
the environment `stage`, for example `vnet-k2bridge-stage`.

In local development, a local state store is used, and resources are named by default with the environment `dev`,
for example `vnet-k2bridge-dev`.

The Azure DevOps pipeline is based on the [Terraform Azure Pipelines Starter](https://github.com/algattik/terraform-azure-pipelines-starter/),
but simplified to remove a separate "terraform plan" stage and manual approvals. It requires installing the
[Terraform extension for Azure DevOps](https://marketplace.visualstudio.com/items?itemName=ms-devlabs.custom-terraform-tasks).
