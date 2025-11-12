using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Blog API", Version = "v1" });
});

// DATABASE: SQL Server (local) | PostgreSQL (Render)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string missing");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<BlogContext>(opt => opt.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<BlogContext>(opt => opt.UseNpgsql(connectionString));
}

var app = builder.Build();

// Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API V1");
    c.RoutePrefix = "swagger";
});

// Health Check (Render)
app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// AUTO CREATE DB (NO MIGRATIONS — LIKE BEFORE)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogContext>();
    try
    {
        context.Database.EnsureCreated(); // This is what you had — it just works
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error creating database");
    }
}

app.Run();