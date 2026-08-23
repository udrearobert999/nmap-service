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

COPY ["src/scans/Vantage.Worker/Vantage.Worker.csproj", "src/scans/Vantage.Worker/"]
COPY ["src/scans/Vantage.Application.Worker/Vantage.Application.Worker.csproj", "src/scans/Vantage.Application.Worker/"]
COPY ["src/scans/Vantage.Infrastructure.Worker/Vantage.Infrastructure.Worker.csproj", "src/scans/Vantage.Infrastructure.Worker/"]
COPY ["src/scans/Vantage.Infrastructure.Persistence/Vantage.Infrastructure.Persistence.csproj", "src/scans/Vantage.Infrastructure.Persistence/"]
COPY ["src/scans/Vantage.Domain/Vantage.Domain.csproj", "src/scans/Vantage.Domain/"]
COPY ["src/scans/Vantage.Contracts/Vantage.Contracts.csproj", "src/scans/Vantage.Contracts/"]
COPY ["tst/Vantage.Worker.Tests/Vantage.Worker.Tests.csproj", "tst/Vantage.Worker.Tests/"]

COPY ["src/scans/", "src/scans/"]
COPY ["tst/", "tst/"]

RUN find src/ tst/ -name "bin*" -type d -exec rm -rf {} + && \
    find src/ tst/ -name "obj*" -type d -exec rm -rf {} +

RUN dotnet restore "src/scans/Vantage.Worker/Vantage.Worker.csproj"
RUN dotnet restore "tst/Vantage.Worker.Tests/Vantage.Worker.Tests.csproj"

RUN dotnet test "tst/Vantage.Worker.Tests/Vantage.Worker.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

WORKDIR "/app/src/scans/Vantage.Worker"
RUN dotnet build "Vantage.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Vantage.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER app
ENTRYPOINT ["dotnet", "Vantage.Worker.dll"]