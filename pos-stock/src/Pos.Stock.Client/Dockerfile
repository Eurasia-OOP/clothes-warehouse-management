FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/Pos.Stock.Client/Pos.Stock.Client.csproj src/Pos.Stock.Client/

RUN dotnet restore "src/Pos.Stock.Client/Pos.Stock.Client.csproj"

COPY src/Pos.Stock.Client/ src/Pos.Stock.Client/

RUN dotnet publish "src/Pos.Stock.Client/Pos.Stock.Client.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish

FROM nginx:1.31-alpine AS final

COPY src/Pos.Stock.Client/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY src/Pos.Stock.Client/docker/40-api-base-url.sh /docker-entrypoint.d/

# Strip CR characters in case the script came from a checkout with CRLF line endings. A CR breaks the shebang.
RUN sed -i 's/\r$//' /docker-entrypoint.d/40-api-base-url.sh \
    && chmod +x /docker-entrypoint.d/40-api-base-url.sh

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
