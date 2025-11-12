# Blog Management Application

A full-stack blog management application built with .NET 9 Web API and Angular.

## Backend (.NET 9 Web API)

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (or SQL Server)

### Setup and Running

1. Navigate to the backend directory:
   ```bash
   cd backend/BlogApi
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Run database migrations (this will create the database and seed initial data):
   ```bash
   dotnet ef database update
   ```
   
   If you encounter an error about missing EF Core tools, install them with:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Run the API:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

### API Endpoints

- `GET /api/BlogPosts` - Get all blog posts (supports pagination and filtering)
- `GET /api/BlogPosts/{id}` - Get a specific blog post by ID
- `POST /api/BlogPosts` - Create a new blog post
- `PUT /api/BlogPosts/{id}` - Update an existing blog post
- `DELETE /api/BlogPosts/{id}` - Delete a blog post

### Swagger UI

When running in development, you can access the Swagger UI at `https://localhost:5001/swagger` to test the API endpoints.

## Frontend (Angular)

### Prerequisites

- [Node.js](https://nodejs.org/) (v18.0.0 or later)
- [Angular CLI](https://angular.io/cli)

### Setup and Running

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the development server:
   ```bash
   ng serve
   ```

The application will be available at `http://localhost:4200`.

## Deployment to Render

### Backend Deployment

1. Create a new Web Service on Render
2. Connect your GitHub repository
3. Set the following environment variables:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection` - Your production database connection string
4. Set the build command: `dotnet publish -c Release -o output`
5. Set the start command: `dotnet BlogApi.dll`

### Frontend Deployment

1. Build the Angular app for production:
   ```bash
   ng build --configuration production
   ```
2. Upload the contents of the `dist/blog-ui` directory to your static site hosting service.

## Features

- Create, read, update, and delete blog posts
- Responsive design
- Pagination and filtering
- RESTful API with proper status codes and error handling
- Swagger API documentation

## Technologies Used

### Backend
- .NET 9 Web API
- Entity Framework Core
- SQL Server
- Swagger/OpenAPI

### Frontend
- Angular 17+
- TypeScript
- RxJS
- Angular Material (for UI components)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
