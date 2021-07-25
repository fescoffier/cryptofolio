FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-alpine3.13 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-alpine3.13 AS build
COPY ["src/", "src/"]
RUN dotnet restore "src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj"
RUN dotnet build "src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Cryptofolio.Handlers.Job/Cryptofolio.Handlers.Job.csproj" -c Release -o /app/publish -r alpine.3.13-x64 -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cryptofolio.Handlers.Job.dll"]
