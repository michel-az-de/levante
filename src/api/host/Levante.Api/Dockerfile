# syntax=docker/dockerfile:1.7
# Build from the repository root: docker build -f src/api/host/Levante.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central Package Management: os props da raiz precisam existir antes do restore.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/api/ src/api/
RUN --mount=type=cache,target=/root/.nuget/packages,id=nuget \
    dotnet restore src/api/host/Levante.Api/Levante.Api.csproj
RUN --mount=type=cache,target=/root/.nuget/packages,id=nuget \
    dotnet publish src/api/host/Levante.Api/Levante.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# A imagem aspnet ja traz um usuario nao-root "app" (uid 1654).
USER app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production \
    Kestrel__Endpoints__Http__Url=http://+:8080

ENTRYPOINT ["dotnet", "Levante.Api.dll"]
