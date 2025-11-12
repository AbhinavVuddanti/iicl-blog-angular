# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy ONLY the project file first
COPY BlogApi.csproj .

# Restore dependencies
RUN dotnet restore BlogApi.csproj

# Copy the rest of the code
COPY . .

# Publish
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
