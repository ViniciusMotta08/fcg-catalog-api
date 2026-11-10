# syntax=docker/dockerfile:1
# ---------- Estágio 1: build (SDK) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro: o restore fica em cache enquanto as dependências não mudarem.
COPY ["src/FCG.Catalog.Domain/FCG.Catalog.Domain.csproj", "src/FCG.Catalog.Domain/"]
COPY ["src/FCG.Catalog.Application/FCG.Catalog.Application.csproj", "src/FCG.Catalog.Application/"]
COPY ["src/FCG.Catalog.Infrastructure/FCG.Catalog.Infrastructure.csproj", "src/FCG.Catalog.Infrastructure/"]
COPY ["src/FCG.Catalog.Api/FCG.Catalog.Api.csproj", "src/FCG.Catalog.Api/"]
RUN dotnet restore "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/FCG.Catalog.Api/FCG.Catalog.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Estágio 2: runtime (somente ASP.NET Core, sem SDK) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

COPY --from=build /app/publish .

# Usuário sem privilégios já existente na imagem oficial do .NET 8.
USER $APP_UID

ENTRYPOINT ["dotnet", "FCG.Catalog.Api.dll"]
