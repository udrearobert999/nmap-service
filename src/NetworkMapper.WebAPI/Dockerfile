FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY ["src/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj", "src/NetworkMapper.WebAPI/"]
COPY ["src/NetworkMapper.Application/NetworkMapper.Application.csproj", "src/NetworkMapper.Application/"]
COPY ["src/NetworkMapper.Infrastructure/NetworkMapper.Infrastructure.csproj", "src/NetworkMapper.Infrastructure/"]
COPY ["src/NetworkMapper.Infrastructure.Persistence/NetworkMapper.Infrastructure.Persistence.csproj", "src/NetworkMapper.Infrastructure.Persistence/"]
COPY ["src/NetworkMapper.Domain/NetworkMapper.Domain.csproj", "src/NetworkMapper.Domain/"]
COPY ["src/NetworkMapper.Contracts/NetworkMapper.Contracts.csproj", "src/NetworkMapper.Contracts/"]
COPY ["tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj", "tst/NetworkMapper.WebAPI.Tests/"]

RUN dotnet restore "src/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj"
RUN dotnet restore "tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj"

COPY . .

RUN dotnet test "tst/NetworkMapper.WebAPI.Tests/NetworkMapper.WebAPI.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

WORKDIR "/src/NetworkMapper.WebAPI"
RUN dotnet build "NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkMapper.WebAPI.dll"]