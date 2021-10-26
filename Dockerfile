# Multi-stage Docker build, test and package
# Based on https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-unit-testing.md

ARG DOTNET_VERSION=5.0

# STAGE: Base build and test
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
WORKDIR /app
ARG VersionPrefix

# Copy csproj and restore as distinct layers. This caches a Docker layer with downloaded
# dependencies, making the next build faster if only code files have been changed.
COPY K2Bridge/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish K2Bridge -c Release -o out /p:VersionPrefix=${VersionPrefix}

# Test for lint issues, UnitTests project causes the main to build too
RUN dotnet build K2Bridge.Tests.UnitTests -p:TreatWarningsAsErrors=true -warnaserror

# Run unit tests. "exit 0" prevents failing build on test failures.
# (dotnet test returns a non-zero exit code on test failures, which would stop the docker build otherwise)
RUN dotnet test K2Bridge.Tests.UnitTests --no-build --logger "trx;LogFileName=/app/TestResult.xml" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura ; exit 0

# Fail build if no test result file produced (indicates an issue with the test runner itself)
RUN test -s /app/TestResult.xml

# Build end2end tests
RUN dotnet build K2Bridge.Tests.End2End /p:TreatWarningsAsErrors=true -warnaserror /p:VersionPrefix=${VersionPrefix}


# STAGE: Build image for executing End2End tests in Kubernetes
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS end2endtest

COPY --from=build /app/K2Bridge ./K2Bridge
COPY --from=build /app/K2Bridge.Tests.End2End ./K2Bridge.Tests.End2End

# Create a FIFO pipe allowing to fetch test outputs before container terminates
RUN mkfifo /test-result-pipe

# Run the created image to execute End2End tests
CMD ["bash", "-x", "-c", "dotnet test K2Bridge.Tests.End2End '--logger:trx;LogFileName=/app/TestResult.xml' ; cat /app/TestResult.xml > /test-result-pipe ; sleep 5"]


# STAGE: Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "K2Bridge.dll"]
