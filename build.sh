#!/bin/bash

echo "Restoring dotnet tools."
dotnet tool restore --tool-manifest .config/dotnet-tools.json

export REGISTRY=registry.digitalocean.com/cryptofolio
export API_BUILD_CONTEXT=${API_BUILD_CONTEXT:-../}
export API_REPOSITORY=${API_REPOSITORY:-api}
export API_VERSION=$(dotnet version -p src/Cryptofolio.Api/Cryptofolio.Api.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "+" commit_sha}')
export API_TAG=$(dotnet version -p src/Cryptofolio.Api/Cryptofolio.Api.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "." commit_sha}')
export APP_BUILD_CONTEXT=${APP_BUILD_CONTEXT:-../}
export APP_REPOSITORY=${APP_REPOSITORY:-app}
export APP_VERSION=$(dotnet version -p src/Cryptofolio.App/Cryptofolio.App.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "+" commit_sha}')
export APP_TAG=$(dotnet version -p src/Cryptofolio.App/Cryptofolio.App.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "." commit_sha}')
export JOB_COLLECTOR_BUILD_CONTEXT=${JOB_COLLECTOR_BUILD_CONTEXT:-../}
export JOB_COLLECTOR_REPOSITORY=${JOB_COLLECTOR_REPOSITORY:-jobs/collector}
export JOB_COLLECTOR_VERSION=$(dotnet version -p src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "+" commit_sha}')
export JOB_COLLECTOR_TAG=$(dotnet version -p src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "." commit_sha}')
export JOB_HANDLERS_CONTEXT=${JOB_HANDLERS_CONTEXT:-../}
export JOB_HANDLERS_REPOSITORY=${JOB_HANDLERS_REPOSITORY:-jobs/handlers}
export JOB_HANDLERS_VERSION=$(dotnet version -p src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "+" commit_sha}')
export JOB_HANDLERS_TAG=$(dotnet version -p src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "." commit_sha}')
export JOB_BALANCES_CONTEXT=${JOB_BALANCES_CONTEXT:-../}
export JOB_BALANCES_REPOSITORY=${JOB_BALANCES_REPOSITORY:-jobs/balances}
export JOB_BALANCES_VERSION=$(dotnet version -p src/Cryptofolio.Balances.Job/Cryptofolio.Balances.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "+" commit_sha}')
export JOB_BALANCES_TAG=$(dotnet version -p src/Cryptofolio.Balances.Job/Cryptofolio.Balances.Job.csproj --show | awk -v commit_sha=$(git rev-parse --short HEAD) '{print $3 "." commit_sha}')

echo -e "\n"
echo "REGISTRY: $REGISTRY"
echo "API_BUILD_CONTEXT: $API_BUILD_CONTEXT"
echo "API_REPOSITORY: $API_REPOSITORY"
echo "API_VERSION: $API_VERSION"
echo "API_TAG: $API_TAG"
echo "APP_BUILD_CONTEXT: $REAPP_BUILD_CONTEXTGISTRY"
echo "APP_REPOSITORY: $APP_REPOSITORY"
echo "APP_VERSION: $APP_VERSION"
echo "APP_TAG: $APP_TAG"
echo "JOB_COLLECTOR_BUILD_CONTEXT: $JOB_COLLECTOR_BUILD_CONTEXT"
echo "JOB_COLLECTOR_REPOSITORY: $JOB_COLLECTOR_REPOSITORY"
echo "JOB_COLLECTOR_VERSION: $JOB_COLLECTOR_VERSION"
echo "JOB_COLLECTOR_TAG: $JOB_COLLECTOR_TAG"
echo "JOB_HANDLERS_CONTEXT: $JOB_HANDLERS_CONTEXT"
echo "JOB_HANDLERS_REPOSITORY: $JOB_HANDLERS_REPOSITORY"
echo "JOB_HANDLERS_VERSION: $JOB_HANDLERS_VERSION"
echo "JOB_HANDLERS_TAG: $JOB_HANDLERS_TAG"
echo "JOB_BALANCES_CONTEXT: $JOB_BALANCES_CONTEXT"
echo "JOB_BALANCES_REPOSITORY: $JOB_BALANCES_REPOSITORY"
echo "JOB_BALANCES_VERSION: $JOB_BALANCES_VERSION"
echo "JOB_BALANCES_TAG: $JOB_BALANCES_TAG"
echo -e "\n"

echo -e "Building images with their version tag.\n"
docker-compose -f docker/docker-compose-build.yaml build --no-rm
if [ $? == 0 ]
then
  docker-compose -f docker/docker-compose-build.yaml push
else
  echo -e "Failed to build the images with their version tag. Push skipped.\n"
  exit 1
fi

export API_TAG=latest
export APP_TAG=latest
export JOB_COLLECTOR_TAG=latest
export JOB_HANDLERS_TAG=latest
export JOB_BALANCES_TAG=latest

echo -e "Building images with the 'latest' tag.\n"
docker-compose -f docker/docker-compose-build.yaml build --no-rm
if [ $? == 0 ]
then
  docker-compose -f docker/docker-compose-build.yaml push
else
  echo -e "Failed to build the images with 'latest' tag. Push skipped.\n"
  exit 1
fi