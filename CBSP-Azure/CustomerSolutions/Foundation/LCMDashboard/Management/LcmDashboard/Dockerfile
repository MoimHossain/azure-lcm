FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AzLcm.Daemon/AzLcm.Daemon.csproj", "AzLcm.Daemon/"]
COPY ["AzLcm.Shared/AzLcm.Shared.csproj", "AzLcm.Shared/"]
RUN dotnet restore "./AzLcm.Daemon/./AzLcm.Daemon.csproj"
COPY . .
WORKDIR "/src/AzLcm.Daemon"
RUN dotnet build "./AzLcm.Daemon.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AzLcm.Daemon.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzLcm.Daemon.dll"]