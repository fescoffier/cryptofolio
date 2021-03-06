#!/bin/bash

docker-compose -f docker/docker-compose-dev.yaml up -d

dotnet restore
dotnet build --no-restore

pushd src/Cryptofolio.App/ClientApp/
npm i --pure-lockfile
popd

_term() {
  echo "Terminating processes."
  kill -TERM $APP_PID $API_PID $JOB_HANDLERS_PID $JOB_BALANCES_PID $JOB_COLLECTOR_PID 2>/dev/null
}

trap _term SIGTERM

export Serilog__MinimumLevel__Default=Information

dotnet run -p src/Cryptofolio.App/ &
APP_PID=$!

dotnet run -p src/Cryptofolio.Api/ &
API_PID=$!

dotnet run -p src/Cryptofolio.Handlers.Job/ &
JOB_HANDLERS_PID=$!

dotnet run -p src/Cryptofolio.Balances.Job/ &
JOB_BALANCES_PID=$!

dotnet run -p src/Cryptofolio.Collector.Job/ &
JOB_COLLECTOR_PID=$!

wait $APP_PID