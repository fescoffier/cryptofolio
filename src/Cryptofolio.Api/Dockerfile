FROM mcr.microsoft.com/dotnet/aspnet:5.0.8-alpine3.13 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-alpine3.13 AS build
COPY ["src/", "src/"]
RUN dotnet restore "src/Cryptofolio.Api/Cryptofolio.Api.csproj"
RUN dotnet build "src/Cryptofolio.Api/Cryptofolio.Api.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION
RUN dotnet publish "src/Cryptofolio.Api/Cryptofolio.Api.csproj" -c Release -o /app/publish -r alpine.3.13-x64 /p:PublishReadyToRun=true /property:Version=$VERSION

FROM base AS final
ARG VERSION
WORKDIR /app
COPY --from=publish /app/publish .
ENV VERSION=$VERSION

ENTRYPOINT ["dotnet", "Cryptofolio.Api.dll"]
