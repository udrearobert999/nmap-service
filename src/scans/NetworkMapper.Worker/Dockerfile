FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    nmap \
    && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["src/scans/NetworkMapper.Worker/NetworkMapper.Worker.csproj", "src/scans/NetworkMapper.Worker/"]
COPY ["src/scans/NetworkMapper.Application.Worker/NetworkMapper.Application.Worker.csproj", "src/scans/NetworkMapper.Application.Worker/"]
COPY ["src/scans/NetworkMapper.Infrastructure.Worker/NetworkMapper.Infrastructure.Worker.csproj", "src/scans/NetworkMapper.Infrastructure.Worker/"]
COPY ["src/scans/NetworkMapper.Infrastructure.Persistence/NetworkMapper.Infrastructure.Persistence.csproj", "src/scans/NetworkMapper.Infrastructure.Persistence/"]
COPY ["src/scans/NetworkMapper.Domain/NetworkMapper.Domain.csproj", "src/scans/NetworkMapper.Domain/"]
COPY ["src/scans/NetworkMapper.Contracts/NetworkMapper.Contracts.csproj", "src/scans/NetworkMapper.Contracts/"]
COPY ["tst/NetworkMapper.Worker.Tests/NetworkMapper.Worker.Tests.csproj", "tst/NetworkMapper.Worker.Tests/"]

COPY ["src/scans/", "src/scans/"]
COPY ["tst/", "tst/"]

RUN find src/ tst/ -name "bin*" -type d -exec rm -rf {} + && \
    find src/ tst/ -name "obj*" -type d -exec rm -rf {} +

RUN dotnet restore "src/scans/NetworkMapper.Worker/NetworkMapper.Worker.csproj"
RUN dotnet restore "tst/NetworkMapper.Worker.Tests/NetworkMapper.Worker.Tests.csproj"

RUN dotnet test "tst/NetworkMapper.Worker.Tests/NetworkMapper.Worker.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

WORKDIR "/app/src/scans/NetworkMapper.Worker"
RUN dotnet build "NetworkMapper.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NetworkMapper.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER app
ENTRYPOINT ["dotnet", "NetworkMapper.Worker.dll"]