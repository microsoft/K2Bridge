# Kibana Kusto Bridge

**A bridge connecting Kibana to Kusto (Azure Data Explorer) as its backend database**

---

[![Build Status](https://dev.azure.com/csedevil/K2-bridge/_apis/build/status/microsoft.KibanaKustoBridge?branchName=master)](https://dev.azure.com/csedevil/K2-bridge/_build/latest?definitionId=146&branchName=master)
[![MIT license](https://img.shields.io/badge/license-MIT-brightgreen.svg)](http://opensource.org/licenses/MIT)

## Description

## Installation

See the Installation document

## Development

Running Kibana and KibanaKustoBridge locally for testing and development, see [here](./docs/development.md)

### Run on Azure Kubernetes Service

```sh

CONTAINER_NAME=[CONTAINER_NAME]
REPOSITORY_NAME=[YOU_REPO_NAME]

docker build -t $CONTAINER_NAME .
docker tag $CONTAINER_NAME $REPOSITORY_NAME/$CONTAINER_NAME
docker push $CONTAINER_NAME $REPOSITORY_NAME/$CONTAINER_NAME


helmv3 upgrade -i [RELEASE_NAME] charts/k2bridge --wait --namespace [NAMESPACE_NAME] --set image.repository=$CONTAINER_NAME $REPOSITORY_NAME/$CONTAINER_NAME

Note: To run on other kubernetes providers, change value.yaml elasticsearch storage class to fit the one suggested by the provider. 
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
