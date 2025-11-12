IICL Blog Management Application

A full-stack Blog Management Application built with .NET 9 Web API and Angular 19.

## Backend (.NET 9 Web API)
### Prerequisites
- .NET 9.0 SDK
- SQLite (for development) or PostgreSQL (for production)

### Setup and Running
Navigate to the backend directory:
```
cd backend/BlogApi
```
Restore NuGet packages:
```
dotnet restore
```
Run database migrations (this will create the SQLite database and apply migrations):
```
dotnet ef database update
```
If you encounter an error about missing EF Core tools, install them with:
```
dotnet tool install --global dotnet-ef
```
Run the API:
```
dotnet run
```
The API will be available at **http://localhost:5162** and Swagger UI at **http://localhost:5162/swagger**.

### API Endpoints
- **GET /api/blogs** — Get all blog posts (supports pagination and filtering)
- **GET /api/blogs/{id}** — Get a specific blog post by ID
- **POST /api/blogs** — Create a new blog post
- **PUT /api/blogs/{id}** — Update an existing blog post
- **DELETE /api/blogs/{id}** — Delete a blog post

### Swagger UI
When running in development, access Swagger at:
```
http://localhost:5162/swagger
```
for testing API endpoints.

---

## Frontend (Angular 19 + Angular Material)
### Prerequisites
- Node.js 18 or later (Angular 19 recommends Node 20+)
- Angular CLI

### Setup and Running
Navigate to the frontend directory:
```
cd frontend
```
Install dependencies:
```
npm install
```
Run the development server:
```
npm start
```
The application will be available at **http://localhost:4200**.

The frontend calls the API at **http://localhost:5162/api**. You can modify this in:
```
src/environments/environment.ts
```

---

## Deployment to Render
### Backend Deployment
1. Create a new **Web Service** on Render and connect your GitHub repository.
2. In the Render settings:
   - Build Command: `dotnet publish -c Release -o out`
   - Start Command: `dotnet out/BlogApi.dll`
3. Set environment variables:
   - `ASPNETCORE_URLS=http://0.0.0.0:10000`
   - `UsePostgres=true`
   - `ConnectionStrings__Postgres=<your_postgres_connection_string>`
   - `Cors__FrontendUrl=https://<your-frontend>.onrender.com`

### Frontend Deployment
1. Build the Angular app for production:
```
npm run build
```
2. Set the API base URL in `src/environments/environment.prod.ts`:
```
export const environment = {
  production: true,
  apiBase: 'https://<your-backend>.onrender.com/api'
};
```
3. Deploy the **dist/blog-admin/browser** folder to Render as a **Static Site**.

---

## Features
- Full CRUD operations for Blog Posts
- Responsive admin UI with Angular Material components
- Validation with consistent JSON error responses
- Pagination and filtering
- Swagger/OpenAPI documentation
- CORS, rate limiting, and logging for security and reliability
- Configurable database: SQLite (local) or PostgreSQL (cloud)

---

## Technologies Used
### Backend
- .NET 9 Web API
- Entity Framework Core
- SQLite / PostgreSQL
- Swagger / OpenAPI
- ASP.NET Middleware (CORS, Logging, Rate Limiting, Error Handling)

### Frontend
- Angular 19
- TypeScript
- Angular Material
- RxJS

---

## Example Requests
**Create a new blog post:**
```
POST /api/blogs
{
  "title": "My First Post",
  "content": "This is my first blog post using .NET and Angular.",
  "author": "Abhinav Vuddanti"
}
```
**Fetch blog posts with pagination:**
```
GET /api/blogs?page=1&pageSize=10&search=post
```

---

## Author
👤 **Abhinav Vuddanti**  
📧 [abhin6289@gmail.com](mailto:abhin6289@gmail.com)

---

Live Urls :

Backend : https://iicl-blog-1.onrender.com
API endpoints :  https://iicl-blog-1.onrender.com/api/blogs
Frontend : https://iicl-blog-1-1.onrender.com


✅ **Summary:**
This project delivers a complete full-stack blog management system with:
- .NET 9 backend (secure, scalable API)
- Angular 19 frontend (responsive and user-friendly UI)
- Ready-to-deploy configuration for Render
- Swagger documentation and robust validation

