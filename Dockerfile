# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything (we're already in backend/BlogApi/)
COPY . .

# Restore & publish using the project file in current directory
RUN dotnet restore BlogApi.csproj
RUN dotnet publish BlogApi.csproj -c Release -o /app/out --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Render uses $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK CMD curl --fail http://localhost:${PORT}/health || exit 1

EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "BlogApi.dll"]
