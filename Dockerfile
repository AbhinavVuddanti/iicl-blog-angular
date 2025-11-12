# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Blog.Api/Blog.Api.csproj"
RUN dotnet publish "Blog.Api/Blog.Api.csproj" -c Release -o /app/out

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
# Render provides PORT. Bind to 0.0.0.0 for external access.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
# Run in Development so the app uses SQLite (no external DB needed)
ENV ASPNETCORE_ENVIRONMENT=Development
CMD ["dotnet", "Blog.Api.dll"]
