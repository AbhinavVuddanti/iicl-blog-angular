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
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create a directory for the SQLite database
RUN mkdir -p /app/Data

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose port 8080
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "BlogApi.dll"]
