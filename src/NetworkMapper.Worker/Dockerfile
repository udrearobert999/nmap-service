FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root

RUN apt-get update && apt-get install -y nmap && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/NetworkMapper.Worker/NetworkMapper.Worker.csproj", "src/NetworkMapper.Worker/"]
COPY ["src/NetworkMapper.Application.Worker/NetworkMapper.Application.Worker.csproj", "src/NetworkMapper.Application.Worker/"]
COPY ["src/NetworkMapper.Infrastructure.Worker/NetworkMapper.Infrastructure.Worker.csproj", "src/NetworkMapper.Infrastructure.Worker/"]
COPY ["src/NetworkMapper.Infrastructure.Persistence/NetworkMapper.Infrastructure.Persistence.csproj", "src/NetworkMapper.Infrastructure.Persistence/"]
COPY ["src/NetworkMapper.Domain/NetworkMapper.Domain.csproj", "src/NetworkMapper.Domain/"]
COPY ["src/NetworkMapper.Contracts/NetworkMapper.Contracts.csproj", "src/NetworkMapper.Contracts/"]

RUN dotnet restore "./src/NetworkMapper.Worker/NetworkMapper.Worker.csproj"
COPY . .
WORKDIR "/src/src/NetworkMapper.Worker"
RUN dotnet build "./NetworkMapper.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NetworkMapper.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkMapper.Worker.dll"]