# ── Stage 1: Restore (cached layer unless .csproj changes) ───────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src
COPY proekt.csproj ./
RUN dotnet restore proekt.csproj --locked-mode || dotnet restore proekt.csproj

# ── Stage 2: Build & Publish ─────────────────────────────────────────────────
FROM restore AS build
COPY . .
RUN dotnet publish proekt.csproj -c Release -o /app/publish --no-restore || \
    dotnet publish proekt.csproj -c Release -o /app/publish

# ── Stage 3: Runtime image ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

USER appuser

# Render requires port 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "proekt.dll"]