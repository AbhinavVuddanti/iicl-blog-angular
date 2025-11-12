using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",    // React dev server
                "https://localhost:3000",   // React dev server (HTTPS)
                "http://localhost:4200",    // Angular dev server
                "https://localhost:4200",   // Angular dev server (HTTPS)
                "http://localhost:5000",    // Backend HTTP
                "https://localhost:5001"    // Backend HTTPS
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Blog API", 
        Version = "v1",
        Description = "API for managing blog posts",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });
});

// Database configuration
builder.Services.AddDbContext<BlogContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=blog.db")
           .ConfigureWarnings(warnings => 
               warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API V1");
        c.RoutePrefix = "swagger";
    });
}

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self';");
    
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName 
}));

// API Controllers
app.MapControllers();

// Serve static files from wwwroot
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (Directory.Exists(wwwrootPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    
    // Handle SPA routing - serve index.html for non-API routes
    app.MapWhen(
        ctx => {
            var path = ctx.Request.Path.Value ?? string.Empty;
            return !path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) && 
                   !path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) &&
                   !path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) &&
                   !path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) &&
                   !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase);
        },
        appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(Path.Combine(wwwrootPath, "index.html"));
            });
        }
    );
}

// Ensure Swagger is available in production
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API V1");
        c.RoutePrefix = "swagger";
    });
    
    // Redirect root to Swagger in production if wwwroot doesn't exist
    if (!Directory.Exists(wwwrootPath))
    {
        app.MapGet("/", () => Results.Redirect("/swagger"));
    }
}

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BlogContext>();
        if (context.Database.IsSqlite())
        {
            // For SQLite, we'll use EnsureCreated for simplicity
            context.Database.EnsureCreated();
            // If you want to use migrations in the future, use:
            // await context.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();