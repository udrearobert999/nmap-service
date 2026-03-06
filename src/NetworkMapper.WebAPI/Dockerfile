FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj", "src/NetworkMapper.WebAPI/"]
COPY ["src/NetworkMapper.Infrastructure/NetworkMapper.Infrastructure.csproj", "src/NetworkMapper.Infrastructure/"]
COPY ["src/NetworkMapper.Application/NetworkMapper.Application.csproj", "src/NetworkMapper.Application/"]
COPY ["src/NetworkMapper.Domain/NetworkMapper.Domain.csproj", "src/NetworkMapper.Domain/"]
RUN dotnet restore "./src/NetworkMapper.WebAPI/NetworkMapper.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/NetworkMapper.WebAPI"
RUN dotnet build "./NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NetworkMapper.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkMapper.WebAPI.dll"]