# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY BlogApi.csproj .
RUN dotnet restore BlogApi.csproj

# Copy source
COPY . .
RUN dotnet publish BlogApi.csproj -c Release -o /app/out --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Render: Use $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (Render requires this)
HEALTHCHECK CMD curl --fail http://localhost:${PORT}/health || exit 1

EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "BlogApi.dll"]
