# ── Etapa 1: build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY TaskFlowPro.Api/TaskFlowPro.Api.csproj                         TaskFlowPro.Api/
COPY TaskFlowPro.Application/TaskFlowPro.Application.csproj         TaskFlowPro.Application/
COPY TaskFlowPro.Domain/TaskFlowPro.Domain.csproj                   TaskFlowPro.Domain/
COPY TaskFlowPro.Infrastructure/TaskFlowPro.Infrastructure.csproj   TaskFlowPro.Infrastructure/

RUN dotnet restore TaskFlowPro.Api/TaskFlowPro.Api.csproj

# Copiar el resto del código y publicar
COPY TaskFlowPro.Api/           TaskFlowPro.Api/
COPY TaskFlowPro.Application/   TaskFlowPro.Application/
COPY TaskFlowPro.Domain/        TaskFlowPro.Domain/
COPY TaskFlowPro.Infrastructure/ TaskFlowPro.Infrastructure/

RUN dotnet publish TaskFlowPro.Api/TaskFlowPro.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Etapa 2: runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway inyecta la variable PORT; ASP.NET Core la lee con ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "TaskFlowPro.Api.dll"]
