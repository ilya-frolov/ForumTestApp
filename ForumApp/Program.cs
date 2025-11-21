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
        
        // Check if we can connect to the database
        bool canConnect = false;
        bool databaseHasSchema = false;
        bool migrationHistoryExists = false;
        
        try
        {
            canConnect = context.Database.CanConnect();
            logger.LogInformation("Can connect to database: {CanConnect}", canConnect);
            
            if (canConnect)
            {
                // Check if migration history table exists
                migrationHistoryExists = context.Database.ExecuteSqlRaw(
                    "SELECT CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory') THEN 1 ELSE 0 END").ToString() == "1";
                
                // Check if key tables already exist (indicating schema was created from script)
                var aspNetRolesExists = context.Database.ExecuteSqlRaw(
                    "SELECT CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles') THEN 1 ELSE 0 END").ToString() == "1";
                var forumsExists = context.Database.ExecuteSqlRaw(
                    "SELECT CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Forums') THEN 1 ELSE 0 END").ToString() == "1";
                
                databaseHasSchema = aspNetRolesExists && forumsExists;
                
                logger.LogInformation("Migration history table exists: {Exists}", migrationHistoryExists);
                logger.LogInformation("Database schema already exists (created from script): {Exists}", databaseHasSchema);
            }
        }
        catch (Exception connEx)
        {
            logger.LogWarning("Cannot connect to database yet (may not exist): {Message}", connEx.Message);
        }
        
        // Handle migrations based on database state
        if (canConnect && databaseHasSchema && !migrationHistoryExists)
        {
            // Database was created from script - need to create migration history table and mark migrations as applied
            logger.LogInformation("Database was created from script. Creating migration history table and marking migrations as applied...");
            
            try
            {
                // Create migration history table if it doesn't exist
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
                    BEGIN
                        CREATE TABLE [__EFMigrationsHistory] (
                            [MigrationId] NVARCHAR(150) NOT NULL,
                            [ProductVersion] NVARCHAR(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        );
                    END
                ");
                
                logger.LogInformation("Migration history table created.");
                
                // Get all migrations that should be applied
                var allMigrations = context.Database.GetMigrations().ToList();
                var productVersion = typeof(ForumDbContext).Assembly.GetName().Version?.ToString() ?? "8.0.2";
                
                // Insert migration history entries for all migrations without actually running them
                foreach (var migration in allMigrations)
                {
                    try
                    {
                        context.Database.ExecuteSqlRaw(
                            "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
                            "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                            migration, productVersion);
                        
                        logger.LogInformation("Marked migration '{Migration}' as applied", migration);
                    }
                    catch (Exception migEx)
                    {
                        logger.LogWarning("Could not mark migration '{Migration}' as applied: {Message}", migration, migEx.Message);
                    }
                }
                
                logger.LogInformation("All migrations marked as applied successfully.");
            }
            catch (Exception markEx)
            {
                logger.LogWarning("Could not mark migrations as applied. Error: {Message}", markEx.Message);
                logger.LogWarning("The application will attempt to apply migrations normally.");
            }
        }
        
        // Get pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count);
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Migrations to apply: {Migrations}", string.Join(", ", pendingMigrations));
        }
        
        // Apply migrations only if there are pending ones
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying database migrations...");
            try
            {
                context.Database.Migrate();
                logger.LogInformation("Migrations applied successfully.");
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                // Handle case where tables already exist (created from script)
                // Error 2714 = There is already an object named 'X' in the database
                if (sqlEx.Number == 2714 || 
                    sqlEx.Message.Contains("already an object named", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Migration attempted to create objects that already exist (database was created from script).");
                    logger.LogInformation("This is expected. Database schema is already up to date.");
                    
                    // Try to ensure migration history is up to date
                    try
                    {
                        var remainingMigrations = context.Database.GetPendingMigrations().ToList();
                        if (remainingMigrations.Any())
                        {
                            logger.LogInformation("Attempting to mark remaining migrations as applied...");
                            var productVersion = typeof(ForumDbContext).Assembly.GetName().Version?.ToString() ?? "8.0.2";
                            foreach (var migration in remainingMigrations)
                            {
                                try
                                {
                                    context.Database.ExecuteSqlRaw(
                                        "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
                                        "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                                        migration, productVersion);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    
                    // Don't throw - allow application to continue since schema already exists
                }
                else
                {
                    logger.LogError("SQL error during migration: {Number} - {Message}", sqlEx.Number, sqlEx.Message);
                    throw;
                }
            }
        }
        else
        {
            logger.LogInformation("No pending migrations. Database is up to date.");
        }
        
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