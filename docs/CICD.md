## Continuous Integration and Continuous Delivery

### CI/CD Pipeline

When run on a pull request or manually on a branch, the pipeline runs the following:

* __Continuous Integration__:
  * Run static security analysis to ensure code security.
  * Build K2Bridge C# code to binaries, running linter to ensure code quality.
  * Run unit tests, generate a test report and a test coverage report.
  * Publish the K2Bridge container and helm chart to Azure Container Registry.
* __Continuous Delivery__:
  * Provision an Azure environment with Azure Kubernetes Service and Kusto (Azure Data Explorer), using Terraform.
  * Provision a database in the Kusto cluster (temporary database per build).
  * Deploy the K2Bridge chart to AKS (using a temporary namespace for the build).
  * Deploy a standalone Elasticsearch chart to AKS, and run parallel end-to-end tests that load data side-by-side in both Kusto and Elasticsearch and query K2Bridge and Elasticsearch, verifying that both endpoints return equivalent results.
  * Clean up by deleting the per-build Kusto database and AKS namespace.

When run on the `main` or `kibana6.8` branch, the pipeline runs as above, with the following additions:

  * Uses a non-temporary Kusto database and AKS namespace, that is not deleted after the build.
  * Promote successful builds by tagging them in ACR, and publishing them to MCR.

The CI/CD Environment on Azure is depicted below. Kibana is not installed by the pipeline,
but can be manually installed on the namespace.

![CI/CD Environment](./images/CICD%20Environment.png)

The CI/CD pipeline runs multiple jobs.
The pipeline jobs (in blue) and their artifacts (in orange) are shown below.
Arrows indicate antecendent dependencies between jobs. Jobs
that are not in an antecedent relationship can run concurrently (as long as enough build agents
are available in Azure Pipelines).

The Cleanup job runs even if antecedent jobs fail.

![CI/CD Pipeline](./images/CICD%20Pipeline.png)

### End-to-end parallel tests

The end-to-end test suite needs to connect to K2Bridge and Elasticsearch which are accessible
only within the AKS cluster. Therefore, the tests are run in a transient container deployed with
AKS.

A `k2bridge-test` container image containing the end-to-end test suite is built in the CI
phase and deployed to ACR (alongside the `k2bridge` runtime image). In the CD Test phase,
the pipeline runs the image as a one-time pod (using `kubectl run --restart=Never`).

The test suite produces a local test result XML file. The pipeline needs to download this file
before the container shut downs and all its local data is deleted. For this purpose, the
container writes the test result XML file into a [named pipe](https://en.wikipedia.org/wiki/Named_pipe). The pipeline jobs reads this named pipe into a local file on the build agent.
This way, only after the pipeline job has fully read the test result XML,  the test process
completes and the container is deleted.

For running the end-to-end test suite on a local development environment, see [the End2End tests section in the developer documentation](development.md).
