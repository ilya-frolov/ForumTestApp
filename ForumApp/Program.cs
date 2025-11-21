using ForumApp.BL.Managers;
using ForumApp.DAL;
using ForumApp.DAL.DbManagers;
using ForumApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ForumDbContext>();
    context.Database.Migrate(); // applies migrations and seeds via HasData
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Forum API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();