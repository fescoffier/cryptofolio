# Cryptofolio

This repository contains the Cryptofolio app. It's an online cryptocurrency web application.
It was initially designed for the [GLG204](https://formation.cnam.fr/rechercher-par-discipline/architectures-logicielles-java-2--208399.kjsp) unit of value by [Fabien PERRONNET](mailto:perronnet-fabien@live.fr).

## ⚙️ Tools

To run this application locally you'll need the [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0#sdk-5.0.400), [NodeJS](https://nodejs.org/en/) and [Docker](https://www.docker.com/products/docker-desktop) with the [docker-compose CLI](https://docs.docker.com/compose/).

To build the application for a production deployment, you'll need [Docker](https://www.docker.com/products/docker-desktop) with the [docker-compose CLI](https://docs.docker.com/compose/).


To deploy the application to a production environment, you'll need [Helm](https://helm.sh) and a running [Kubernetes](https://kubernetes.io) cluster.
## 🚀 Running the project

The simpliest way to run the application is by using the `run.sh` script.

```bash
chmod +x run.sh
./run.sh
```

It will warm up development infrastructure using Docker, restore Nuget and NPM packages, and run all the microservices in a detached mode. The script will listen for SIGTERM and forward the signal to the processes it started in detached mode.

## 🍳 Building the application for production deployment

The application is designed to run in a containerized environement using Docker images.

The simpliest way to build the images is by using the `build.sh` script.

```bash
chmod +x build.sh
./build.sh
```

## 🖥️ Deploying the application to production

The application is designed to run in a containerized environement, typically a Kubernetes cluster. For that, you can use the existing Helm chart.

Before deploying the chart, create the following secrets :

```bash
kubectl create secret generic connection-strings --from-literal=cryptofolio-context=$CONNECTIONSTRINGS_CRYPTOFOLIOCONTEXT --from-literal=identity-context=$CONNECTIONSTRINGS_IDENTITYCONTEXT
kubeclt create secret generic postgres-passwords --from-literal=postgresql-password=$POSTGRESQL_PASSWORD --from-literal=postgresql-postgres-password=$POSTGRESQL_POSTGRES_PASSWORD --from-literal=repmgr-password=$REPMGR_PASSWORD
kubectl create secret generic cryptofolio-app-users --from-file=/path/to/appsettings.Users.json
kubectl create secret generic fixer --from-literal=apiKey=$FIXER_APIKEY
```

Then deploy the underlying infrastructure :

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo add elastic https://helm.elastic.co
helm repo update
helm install -f charts/bitnami-postgresql-ha/values.yaml postgres bitnami/postgresql-ha
helm install -f charts/bitnami-kafka/values.yaml kafka bitnami/kafka
helm install -f charts/bitnami-zookeeper/values.yaml zookeeper bitnami/zookeeper
helm install -f charts/bitnami-redis/values.yaml redis bitnami/redis
helm install -f charts/elasticsearch/values.yaml elasticsearch elastic/elasticsearch
```

Deploy the [Jetstack Cert Manager](https://cert-manager.io/) following the [docs](https://cert-manager.io/docs/).

Finally, deploy the application chart :

```bash
helm install cryptofolio charts/cryptofolio/
```