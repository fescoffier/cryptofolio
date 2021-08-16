#!/bin/bash

echo "Restoring dotnet tools."
dotnet tool restore --tool-manifest .config/dotnet-tools.json

export REGISTRY=registry.digitalocean.com/cryptofolio
export API_BUILD_CONTEXT=${API_BUILD_CONTEXT:-../}
export API_REPOSITORY=${API_REPOSITORY:-api}
export API_VERSION=$(dotnet version -p src/Cryptofolio.Api/Cryptofolio.Api.csproj --show | awk '{print $3}')-build.$GITHUB_RUN_NUMBER
export APP_BUILD_CONTEXT=${APP_BUILD_CONTEXT:-../}
export APP_REPOSITORY=${APP_REPOSITORY:-app}
export APP_VERSION=$(dotnet version -p src/Cryptofolio.App/Cryptofolio.App.csproj --show | awk '{print $3}')-build.$GITHUB_RUN_NUMBER
export JOB_COLLECTOR_BUILD_CONTEXT=${JOB_COLLECTOR_BUILD_CONTEXT:-../}
export JOB_COLLECTOR_REPOSITORY=${JOB_COLLECTOR_REPOSITORY:-jobs/collector}
export JOB_COLLECTOR_VERSION=$(dotnet version -p src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj --show | awk '{print $3}')-build.$GITHUB_RUN_NUMBER
export JOB_HANDLERS_CONTEXT=${JOB_HANDLERS_CONTEXT:-../}
export JOB_HANDLERS_REPOSITORY=${JOB_HANDLERS_REPOSITORY:-jobs/handlers}
export JOB_HANDLERS_VERSION=$(dotnet version -p src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj --show | awk '{print $3}')-build.$GITHUB_RUN_NUMBER

echo -e "\n"
echo "REGISTRY: $REGISTRY"
echo "API_BUILD_CONTEXT: $API_BUILD_CONTEXT"
echo "API_REPOSITORY: $API_REPOSITORY"
echo "API_VERSION: $API_VERSION"
echo "APP_BUILD_CONTEXT: $REAPP_BUILD_CONTEXTGISTRY"
echo "APP_REPOSITORY: $APP_REPOSITORY"
echo "APP_VERSION: $APP_VERSION"
echo "JOB_COLLECTOR_BUILD_CONTEXT: $JOB_COLLECTOR_BUILD_CONTEXT"
echo "JOB_COLLECTOR_REPOSITORY: $JOB_COLLECTOR_REPOSITORY"
echo "JOB_COLLECTOR_VERSION: $JOB_COLLECTOR_VERSION"
echo "JOB_HANDLERS_CONTEXT: $JOB_HANDLERS_CONTEXT"
echo "JOB_HANDLERS_REPOSITORY: $JOB_HANDLERS_REPOSITORY"
echo "JOB_HANDLERS_VERSION: $JOB_HANDLERS_VERSION"
echo -e "\n"

export API_TAG=$API_VERSION
export APP_TAG=$APP_VERSION
export JOB_COLLECTOR_TAG=$JOB_COLLECTOR_VERSION
export JOB_HANDLERS_TAG=$JOB_HANDLERS_VERSION

echo -e "Building images with their version tag.\n"
docker-compose -f docker/docker-compose-build.yaml build --no-rm
if [ $? == 0 ]
then
  docker-compose -f docker/docker-compose-build.yaml push
else
  echo -e "Failed to build the images with their version tag. Push skipped.\n"
fi

export API_TAG=latest
export APP_TAG=latest
export JOB_COLLECTOR_TAG=latest
export JOB_HANDLERS_TAG=latest

echo -e "Building images with the 'latest' tag.\n"
docker-compose -f docker/docker-compose-build.yaml build --no-rm
if [ $? == 0 ]
then
  docker-compose -f docker/docker-compose-build.yaml push
else
  echo -e "Failed to build the images with 'latest' tag. Push skipped.\n"
fi