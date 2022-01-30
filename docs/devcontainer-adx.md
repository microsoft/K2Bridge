# Azure Data Explorer - Developement environment

K2Bridge requires an access to [Azure Data Explorer](https://azure.microsoft.com/en-us/services/data-explorer/).

If you have already a dev environment with ADX, you can ignore this documentation and directly check the [development](./development.md) instructions to configure K2Bridge application settings.
If not, you can create Data Explorer cluster and database manually -or- use [create-adx.sh](./adx/create-adx.sh) script to deploy all resources needed.

Following steps are performed by this script: 

- [x] Create resource group
- [x] Create Azure Data Explorer cluster
- [x] Create Azure Data Explorer database
- [x] Create service principal
- [x] Assign service principal to database permissions (Viewer role)
- [x] Create Azure Data Explorer table
- [x] Ingest sample data
- [x] Add environment variables to .bashrc (will be used by end-to-end tests)

Note: This script requires jq; this library is installed by default on the dev container. 

### Login with Azure CLI
```bash
vscode ➜ /workspaces/K2Bridge (feature/script-adx-install ✗) $ az login
The default web browser has been opened at https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize. Please continue the login in the web browser. If no web browser is available or if the web browser fails to open, use device code flow with `az login --use-device-code`.
[...]
```

### Ensure your default account is the correct one
```bash
vscode ➜ /workspaces/K2Bridge (feature/script-adx-install ✗) $ az account show
```

### Change directory to .devcontainer/adx
```bash
vscode ➜ /workspaces/K2Bridge (feature/script-adx-install ✗) $ cd ./.devcontainer/adx
```

### Execute create-adx.sh with a unique name and location.
The unique name will be used in resource group, cluster and service principal names. If needed, you can change default naming convention directly in create-adx.sh.

- RESOURCE_GROUP_NAME="rg-k2-$unique-dev"
- ADX_CLUSTER_NAME="adxk2$unique"
- ADX_DB_NAME="devdatabase"
- SERVICE_PRINCIPAL_NAME="sp-k2-$unique"

For location, values can be listed with `az account list-location`. Only location name with no space is valid (for instance westeurope or eastus). 

```bash
vscode ➜ /workspaces/K2Bridge/.devcontainer/adx (feature/script-adx-install ✗) $ bash create-adx.sh -u <uniquename> -l <location>
[...]
Use following settings/secrets in appsettings.development.json:
aadClientId: 00000000-0000-0000-0000-000000000000
aadClientSecret: tuUuUuUuU-JXx~x~xxxxxxxxxxxJ00000_
aadTenantId: 00000000-0000-0000-0000-000000000000
adxClusterUrl: https://adxk2<uniquename>.<location>.kusto.windows.net
adxDefaultDatabaseName: dev
```

You have now a running instance of Azure Data Explorer. Please save output data, you'll need them.

You can now check the [development](./development.md) instructions to configure K2Bridge application settings.
