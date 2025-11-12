<<<<<<< HEAD
# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the .NET project from backend/BlogApi/
COPY backend/BlogApi/BlogApi.csproj ./BlogApi.csproj

# Restore
RUN dotnet restore BlogApi.csproj

# Copy the rest of the code
COPY backend/BlogApi/ .

# Publish
RUN dotnet publish BlogApi.csproj -c Release -o /app/out --no-restore

# --- Runtime stage ---
=======
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY ["backend/BlogApi/BlogApi.csproj", "backend/BlogApi/"]
COPY ["backend/BlogApp.sln", "backend/"]
RUN dotnet restore "backend/BlogApi/BlogApi.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/backend/BlogApi"
RUN dotnet publish "BlogApi.csproj" -c Release -o /app/publish

# Runtime stage
>>>>>>> e185242536ed6930e8d6018c89befa27b76f0a5b
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

<<<<<<< HEAD
# Render uses $PORT
=======
# Set environment variables
>>>>>>> e185242536ed6930e8d6018c89befa27b76f0a5b
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
<<<<<<< HEAD
HEALTHCHECK CMD curl --fail http://localhost:${PORT}/health || exit 1
=======
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:${PORT}/health || exit 1
>>>>>>> e185242536ed6930e8d6018c89befa27b76f0a5b

EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "BlogApi.dll"]
