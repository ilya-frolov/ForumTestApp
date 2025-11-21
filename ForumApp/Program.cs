using ForumApp.BL.Managers;
using ForumApp.DAL;
using ForumApp.DAL.DbManagers;
using ForumApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ForumDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("ForumApp")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ForumDbContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/Account/login";
        options.LogoutPath = "/api/Account/logout";
        options.AccessDeniedPath = "/api/Account/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Add memory cache for post caching
builder.Services.AddMemoryCache();

// Register repositories
builder.Services.AddScoped<IForumsDB, ForumsDB>();
builder.Services.AddScoped<IPostsDB, PostsDB>();
builder.Services.AddScoped<ICommentsDB, CommentsDB>();

// Register business logic managers
builder.Services.AddScoped<ForumManager>();
builder.Services.AddScoped<PostManager>();
builder.Services.AddScoped<CommentManager>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Forum API",
        Description = "A simple forum web application API",
        Contact = new OpenApiContact
        {
            Name = "Forum API Support"
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database (apply migrations)
// Migrate() will create the database if it doesn't exist and apply all migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ForumDbContext>();

    try
    {
        logger.LogInformation("Starting database migration...");
        
        // Log connection string (mask password)
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connString))
        {
            var maskedConnString = connString.Contains("Password=") 
                ? connString.Substring(0, connString.IndexOf("Password=") + 9) + "***" 
                : connString;
            logger.LogInformation("Connection string: {ConnectionString}", maskedConnString);
        }
        
        // Check if we can connect to the database (may return false if database doesn't exist yet)
        try
        {
            var canConnect = context.Database.CanConnect();
            logger.LogInformation("Can connect to database: {CanConnect}", canConnect);
        }
        catch (Exception connEx)
        {
            logger.LogWarning("Cannot connect to database yet (may not exist): {Message}", connEx.Message);
        }
        
        // Get pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count);
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Migrations to apply: {Migrations}", string.Join(", ", pendingMigrations));
        }
        
        // Migrate() will:
        // 1. Create the database if it doesn't exist
        // 2. Apply all pending migrations
        // 3. Seed data via HasData in OnModelCreating
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        
        logger.LogInformation("Database migration completed successfully.");
        
        // Verify forums were seeded
        var forumCount = context.Forums.Count();
        logger.LogInformation("Forums in database: {Count}", forumCount);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        logger.LogError("Error details: {ErrorMessage}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        throw; // Re-throw to prevent app from starting with broken database
    }
}

// Configure the HTTP request pipeline
// Enable Swagger in both Development and Production (can be restricted to Development only for security)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Forum API v1");
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();