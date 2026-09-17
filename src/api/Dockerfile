# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /zeins.order-mng.api/app

COPY ["api/api.csproj", "api/"]
COPY ["core/core.csproj", "core/"]

RUN dotnet restore "api/api.csproj"

COPY ["api/", "api/"]
COPY ["core/", "core/"]

RUN dotnet publish "api/api.csproj" -c Release -o /zeins.order-mng.api/app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /zeins.order-mng.stage/api
RUN mkdir -p /zeins.order-mng.stage/api/data

EXPOSE 8080

COPY --from=build /zeins.order-mng.api/app/publish .

ENTRYPOINT ["dotnet", "api.dll"]