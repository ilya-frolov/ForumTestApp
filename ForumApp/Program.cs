using ForumApp.BL.Managers;
using ForumApp.DAL;
using ForumApp.DAL.DbManagers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Services configuration
// ============================================

// Controllers
builder.Services.AddControllers();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ForumDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("ForumApp")));

// Identity
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

// Authentication
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

// Register DBManagers
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

// ============================================
// Database migration at startup
// ============================================

// Migrate() will create the database if it doesn't exist and apply all migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ForumDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");

    try
    {
        // Quick check: can EF connect?
        if (!context.Database.CanConnect())
        {
            logger.LogInformation("Cannot connect to DB; calling Migrate() to create DB and schema.");
            context.Database.Migrate();
            logger.LogInformation("Database created and migrations applied.");
            return;
        }

        // If DB exists, check whether key tables exist (script-provisioned) and whether migrations history exists
        bool aspNetRolesExists, forumsExists, migrationsHistoryExists;
        using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connString))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                                SELECT
                                 (CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles') THEN 1 ELSE 0 END) AS AspNetRolesExists,
                                 (CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Forums') THEN 1 ELSE 0 END) AS ForumsExists,
                                 (CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory') THEN 1 ELSE 0 END) AS MigrationsHistoryExists;
                                ";
            using var rdr = cmd.ExecuteReader();
            rdr.Read();
            aspNetRolesExists = Convert.ToInt32(rdr["AspNetRolesExists"]) == 1;
            forumsExists = Convert.ToInt32(rdr["ForumsExists"]) == 1;
            migrationsHistoryExists = Convert.ToInt32(rdr["MigrationsHistoryExists"]) == 1;
        }

        // If script-provisioned schema exists but migration history is missing => create history and mark migrations as applied
        if (aspNetRolesExists && forumsExists && !migrationsHistoryExists)
        {
            logger.LogInformation("Scripted schema detected with no __EFMigrationsHistory. Creating history and marking migrations as applied...");

            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            // create __EFMigrationsHistory if missing
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory')
                                    BEGIN
                                        CREATE TABLE [__EFMigrationsHistory] (
                                            [MigrationId] NVARCHAR(150) NOT NULL,
                                            [ProductVersion] NVARCHAR(32) NOT NULL,
                                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                                        );
                                    END";
                cmd.ExecuteNonQuery();
            }

            // insert all migrations as applied
            var migrations = context.Database.GetMigrations().ToList();
            var productVersion = typeof(ForumDbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";
            foreach (var mig in migrations)
            {
                using var insert = conn.CreateCommand();
                insert.Transaction = tx;
                insert.CommandText = @"
                                        IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @m)
                                            INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (@m, @v);";
                insert.Parameters.AddWithValue("@m", mig);
                insert.Parameters.AddWithValue("@v", productVersion);
                insert.ExecuteNonQuery();
                logger.LogInformation("Marked migration applied: {Migration}", mig);
            }

            tx.Commit();
            logger.LogInformation("Migration history populated. Skipping migration execution because schema already exists.");
            return; // skip Migrate() — schema already provided by script and history aligned
        }

        // Normal path: DB exists and either history present or schema not from script — apply pending migrations
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database is up to date.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// ============================================
// Middleware pipeline
// ============================================

// Enable Swagger in all Environments (can be restricted to Development only for security)
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
