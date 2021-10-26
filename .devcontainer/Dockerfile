FROM ubuntu:20.10

ARG USERNAME=dev
ARG USER_UID=1000
ARG USER_GID=$USER_UID

ARG BUILD_ELASTIC_STACK_VERSION="6.8.20"
ARG BUILD_TERRAFORM_VERSION="1.0.9"

# Update & Install common tools
RUN apt-get update && \ 
    apt-get upgrade -y && \
    apt-get install -y sudo git curl wget unzip apt-transport-https default-jre

# Add a non-root user
RUN groupadd --gid $USER_GID $USERNAME && \
    useradd --uid $USER_UID --gid $USER_GID -m $USERNAME && \
    echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME && \
    chmod 0440 /etc/sudoers.d/$USERNAME

# Install Terraform
ENV TERRAFORM_VERSION=${BUILD_TERRAFORM_VERSION}

RUN curl -Os https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip && \
    unzip -o terraform_${TERRAFORM_VERSION}_linux_amd64.zip -d /usr/local/bin
RUN rm terraform_${TERRAFORM_VERSION}_linux_amd64.zip

# Install Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Install Helm
RUN curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3
RUN chmod 700 get_helm.sh
RUN ./get_helm.sh

# Install .NET SDK
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt-get update && \
    apt-get install -y dotnet-sdk-3.1

# Set non-root user as default container user
USER $USERNAME
ENV HOME=/home/$USERNAME
WORKDIR $HOME

# Install ElasticSearch
ENV ELASTIC_STACK_VERSION=${BUILD_ELASTIC_STACK_VERSION}

RUN wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz
RUN wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz.sha512
RUN shasum -a 512 -c elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz.sha512
RUN tar -xzf elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz
RUN rm elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz
RUN rm elasticsearch-oss-${ELASTIC_STACK_VERSION}.tar.gz.sha512

# Install Kibana
RUN wget https://artifacts.elastic.co/downloads/kibana/kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz
RUN wget https://artifacts.elastic.co/downloads/kibana/kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512
RUN shasum -a 512 -c kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512
RUN tar -xzf kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz
RUN rm kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz
RUN rm kibana-oss-${ELASTIC_STACK_VERSION}-linux-x86_64.tar.gz.sha512

CMD [ "sleep", "infinity" ]