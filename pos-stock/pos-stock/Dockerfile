FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Pos.Stock.sln ./
COPY src/Pos.Stock/Pos.Stock.csproj src/Pos.Stock/

RUN dotnet restore "src/Pos.Stock/Pos.Stock.csproj"

COPY . .

WORKDIR /src/src/Pos.Stock

RUN dotnet build "Pos.Stock.csproj" \
    -c Release \
    --no-restore \
    -o /app/build

FROM build AS publish

RUN dotnet publish "Pos.Stock.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Pos.Stock.dll"]
