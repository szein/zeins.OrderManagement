# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /zeins.order-mng.web/app

COPY ["web/WebApp.csproj", "web/"]
COPY ["core/core.csproj", "core/"]

RUN dotnet restore "web/WebApp.csproj"

COPY ["web/", "web/"]
COPY ["core/", "core/"]

RUN dotnet publish "web/WebApp.csproj" -c Release -o /zeins.order-mng.web/app/publish

# Runtime stage
FROM nginx:alpine

COPY web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /zeins.order-mng.web/app/publish/wwwroot /usr/share/nginx/html

COPY web/entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]