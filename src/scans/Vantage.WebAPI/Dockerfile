FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    curl \
    && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

COPY ["src/scans/Vantage.WebAPI/Vantage.WebAPI.csproj", "src/scans/Vantage.WebAPI/"]
COPY ["src/scans/Vantage.Application/Vantage.Application.csproj", "src/scans/Vantage.Application/"]
COPY ["src/scans/Vantage.Infrastructure/Vantage.Infrastructure.csproj", "src/scans/Vantage.Infrastructure/"]
COPY ["src/scans/Vantage.Infrastructure.Persistence/Vantage.Infrastructure.Persistence.csproj", "src/scans/Vantage.Infrastructure.Persistence/"]
COPY ["src/scans/Vantage.Domain/Vantage.Domain.csproj", "src/scans/Vantage.Domain/"]
COPY ["src/scans/Vantage.Contracts/Vantage.Contracts.csproj", "src/scans/Vantage.Contracts/"]
COPY ["tst/Vantage.WebAPI.Tests/Vantage.WebAPI.Tests.csproj", "tst/Vantage.WebAPI.Tests/"]

COPY ["src/scans/", "src/scans/"]
COPY ["tst/", "tst/"]

# TODO: Find fix to remove this
RUN find src/ tst/ -name "bin*" -type d -exec rm -rf {} + && \
    find src/ tst/ -name "obj*" -type d -exec rm -rf {} +

RUN dotnet restore "src/scans/Vantage.WebAPI/Vantage.WebAPI.csproj"
RUN dotnet restore "tst/Vantage.WebAPI.Tests/Vantage.WebAPI.Tests.csproj"

RUN dotnet test "tst/Vantage.WebAPI.Tests/Vantage.WebAPI.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

WORKDIR /app/src/scans/Vantage.WebAPI
RUN dotnet build "Vantage.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Vantage.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vantage.WebAPI.dll"]