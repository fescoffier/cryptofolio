FROM mcr.microsoft.com/dotnet/aspnet:5.0.9-alpine3.13 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.400-alpine3.13 AS build
COPY ["src/", "src/"]
RUN dotnet restore "src/Cryptofolio.App/Cryptofolio.App.csproj"
RUN dotnet build "src/Cryptofolio.App/Cryptofolio.App.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION
RUN dotnet publish "src/Cryptofolio.App/Cryptofolio.App.csproj" -c Release -o /app/publish -r alpine.3.13-x64 /p:PublishReadyToRun=true /property:Version=$VERSION

FROM node:14-alpine as front
ARG VERSION
COPY ["src/Cryptofolio.App/ClientApp", "/src"]
WORKDIR /src
RUN npm install --pure-lockfile
RUN npm run build:prod

FROM base AS final
ARG VERSION
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=front /src/dist ./ClientApp/dist
ENV VERSION=$VERSION

ENTRYPOINT ["dotnet", "Cryptofolio.App.dll"]
