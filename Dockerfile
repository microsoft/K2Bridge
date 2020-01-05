# Multi-stage Docker build, test and package
# Based on https://github.com/dotnet/dotnet-docker/blob/master/samples/dotnetapp/dotnet-docker-unit-testing.md

ARG DOTNET_VERSION=3.0

FROM mcr.microsoft.com/dotnet/core/sdk:$DOTNET_VERSION AS build
WORKDIR /app

# Copy csproj and restore as distinct layers. This caches a Docker layer with downloaded
# dependencies, making the next build faster if only code files have been changed.
COPY K2Bridge/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish K2Bridge -c Release -o out

# Run tests
RUN dotnet test K2Bridge.Tests.UnitTests "--logger:trx;LogFileName=/app/TestResult.xml" 

# Build runtime image

FROM mcr.microsoft.com/dotnet/core/aspnet:$DOTNET_VERSION AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "K2Bridge.dll"]
