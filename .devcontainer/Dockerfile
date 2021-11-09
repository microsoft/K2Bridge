FROM mcr.microsoft.com/vscode/devcontainers/dotnet:6.0-focal

ARG BUILD_ELASTIC_STACK_VERSION="6.8.20"
ARG BUILD_TERRAFORM_VERSION="1.0.9"

# Update & Install Open JDK
RUN apt-get update && apt-get upgrade -y && apt-get install -y default-jre

# Install Terraform
ENV TERRAFORM_VERSION=${BUILD_TERRAFORM_VERSION}
RUN curl -Os https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip -o terraform_${TERRAFORM_VERSION}_linux_amd64.zip -d /usr/local/bin && \
    rm terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Install Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Install Helm
RUN curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3 && \
    chmod 700 get_helm.sh && \
    ./get_helm.sh

# Install mitmproxy
ENV MITMPROXY_VERSION=${BUILD_MITMPROXY_VERSION}
RUN wget https://snapshots.mitmproxy.org/${MITMPROXY_VERSION}/mitmproxy-${MITMPROXY_VERSION}-linux.tar.gz && \
    tar -xzf mitmproxy-${MITMPROXY_VERSION}-linux.tar.gz -C /usr/local/bin && \
    rm mitmproxy-${MITMPROXY_VERSION}-linux.tar.gz

# Set current working directory to /home/vscode
USER vscode
ENV HOME=/home/vscode
WORKDIR $HOME

# Install ElasticSearch
ENV ELASTIC_STACK_VERSION=${BUILD_ELASTIC_STACK_VERSION}

RUN wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz  && \
    wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512 && \
    shasum -a 512 -c elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512 && \
    tar -xzf elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz && \
    rm elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz && \
    rm elasticsearch-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512

# Install Kibana
RUN wget https://artifacts.elastic.co/downloads/kibana/kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz && \
    wget https://artifacts.elastic.co/downloads/kibana/kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz.sha512 && \
    shasum -a 512 -c kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz.sha512 && \
    tar -xzf kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz && \
    rm kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz && \
    rm kibana-oss-${BUILD_KIBANA_VERSION}-linux-x86_64.tar.gz.sha512

CMD [ "sleep", "infinity" ]