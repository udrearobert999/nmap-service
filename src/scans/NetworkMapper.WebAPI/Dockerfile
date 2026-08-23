FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["src/scans/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj", "src/scans/NetworkMapper.WebAPI/"]
COPY ["src/scans/NetworkMapper.Application/NetworkMapper.Application.csproj", "src/scans/NetworkMapper.Application/"]
COPY ["src/scans/NetworkMapper.Infrastructure/NetworkMapper.Infrastructure.csproj", "src/scans/NetworkMapper.Infrastructure/"]
COPY ["src/scans/NetworkMapper.Infrastructure.Persistence/NetworkMapper.Infrastructure.Persistence.csproj", "src/scans/NetworkMapper.Infrastructure.Persistence/"]
COPY ["src/scans/NetworkMapper.Domain/NetworkMapper.Domain.csproj", "src/scans/NetworkMapper.Domain/"]
COPY ["src/scans/NetworkMapper.Contracts/NetworkMapper.Contracts.csproj", "src/scans/NetworkMapper.Contracts/"]
COPY ["tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj", "tst/NetworkMapper.WebAPI.Tests/"]

COPY ["src/scans/", "src/scans/"]
COPY ["tst/", "tst/"]

# TODO: Find fix to remove this
RUN find src/ tst/ -name "bin*" -type d -exec rm -rf {} + && \
    find src/ tst/ -name "obj*" -type d -exec rm -rf {} +

RUN dotnet restore "src/scans/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj"
RUN dotnet restore "tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj"

RUN dotnet test "tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

WORKDIR /app/src/scans/NetworkMapper.WebAPI
RUN dotnet build "NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkMapper.WebAPI.dll"]