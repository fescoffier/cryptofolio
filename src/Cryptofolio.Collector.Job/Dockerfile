FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-buster-slim AS build
COPY ["src/", "src/"]
RUN dotnet restore "src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj"
RUN dotnet build "src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Cryptofolio.Collector.Job/Cryptofolio.Collector.Job.csproj" -c Release -o /app/publish -r debian.10-x64 -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /app
ENTRYPOINT ["dotnet", "Cryptofolio.Collector.Job.dll"]
