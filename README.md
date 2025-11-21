# Forum Web Application

A simple forum web application built with ASP.NET Core (C#), using Entity Framework Core for data access and SQL Server as the database.

## Architecture

The project follows a clean architecture pattern with proper separation of concerns:

### Project Structure

- **ForumApp** - Main web API project
  - Controllers (Account, Forums, Posts, Comments)
  - Program.cs configuration
  - Utils (DtoMapper)
  - Migrations (EF Core migrations)

- **ForumApp.BL** - Business Logic Layer
  - ForumManager
  - PostManager (with caching support)
  - CommentManager

- **ForumApp.DAL** - Data Access Layer
  - ForumDbContext (EF Core DbContext)
  - DbManagers (ForumsDB, PostsDB, CommentsDB)
  - Interfaces (IForumsDB, IPostsDB, ICommentsDB)
  - DBMigrations (SQL migration scripts)

- **ForumApp.Data** - Data Entities
  - Forum entity
  - Post entity
  - Comment entity

- **ForumApp.Models** - Data Transfer Objects (DTOs)
  - DTOs for API requests/responses
  - Error response DTOs

## Features

✅ **User Registration and Authentication**
- ASP.NET Identity with cookie authentication
- Registration endpoint: `POST /api/Account/register`
- Login endpoint: `POST /api/Account/login`
- Logout endpoint: `POST /api/Account/logout`

✅ **Three Pre-configured Forums**
Automatically seeded on first run via EF Core `HasData()`:
- **General Discussion** - Talk about anything
- **Tech Talk** - Discuss technology and programming
- **Off Topic** - Casual conversations and fun

✅ **Post Management**
- Create posts: `POST /api/Posts`
- Get posts by forum with pagination: `GET /api/Posts/forum/{forumId}?skip=0&take=10`
- Get first posts (cached): `GET /api/Posts/first?count=10`
- Get post by ID: `GET /api/Posts/{id}`
- Update posts (owner only): `PUT /api/Posts/{id}`
- **Note:** Posts cannot be deleted (delete functionality removed)

✅ **Comment Management**
- Create comments: `POST /api/Comments`
- Get comments by post: `GET /api/Comments/post/{postId}`
- Get comment by ID: `GET /api/Comments/{id}`
- Delete comments (post owner only): `DELETE /api/Comments/{id}`

✅ **Caching Mechanism**
- In-memory caching for first posts endpoint
- Cache key: `FirstPosts_{count}`
- Cache duration: 5 minutes absolute expiration, 2 minutes sliding expiration
- Automatic cache invalidation on post create/update

## Technologies

- **Server-side**: ASP.NET Core 8.0 (C#)
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: ASP.NET Identity with Cookie Authentication
- **Caching**: Microsoft.Extensions.Caching.Memory (in-memory caching)
- **API Documentation**: Swagger/OpenAPI

## Database Setup

### Environment-Based Configuration

The application automatically selects the configuration file based on the environment:

- **Development Environment** (`ASPNETCORE_ENVIRONMENT=Development`):
  - Uses `appsettings.Development.json` (overrides `appsettings.json`)
  - Set by default when running from Visual Studio or `dotnet run`
  - Can be checked in `ForumApp/Properties/launchSettings.json`

- **Production/Other Environments**:
  - Uses `appsettings.json` only
  - Set by environment variable `ASPNETCORE_ENVIRONMENT=Production` or other value

**How to check/set environment:**
- **Development** (default in Visual Studio): Already set in `launchSettings.json`
- **Production**: Set environment variable `ASPNETCORE_ENVIRONMENT=Production` before running
- **Command line**: `set ASPNETCORE_ENVIRONMENT=Development` (Windows) or `export ASPNETCORE_ENVIRONMENT=Development` (Linux/Mac)

**Configuration Loading Order:**
1. `appsettings.json` - Base configuration (always loaded)
2. `appsettings.{Environment}.json` - Environment-specific overrides (loaded if environment matches)
3. Environment variables - Can override both files
4. Command-line arguments - Highest priority

### Database Setup Options

There are several options for setting up the database. Choose the one that best fits your environment:

### Option 1: Using LocalDB (Recommended for Development)

LocalDB is a lightweight version of SQL Server that runs as a user process. It's perfect for local development and doesn't require a separate SQL Server installation.

**Steps:**

1. **Ensure LocalDB is installed** (comes with Visual Studio or install SQL Server Express)

2. **Update connection string based on your environment:**
   
   **For Development** (uses `appsettings.Development.json`):
   ```json
   // File: appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ForumAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Information"
       }
     }
   }
   ```
   
   **For Production** (uses `appsettings.json`):
   ```json
   // File: appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ForumAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Run the application** - The application will:
   - Automatically detect the environment (Development or Production)
   - Load the appropriate configuration file
   - EF Core will automatically:
     - Create the database if it doesn't exist
     - Apply all migrations
     - Seed the 3 forums via `HasData()` configuration

**Note:** If running in Development mode (default), `appsettings.Development.json` will override `appsettings.json`.

### Option 2: Using SQL Server / SQL Server Express (Allowing EF Core to Build Database)

For SQL Server or SQL Server Express instances, you can let EF Core automatically create and migrate the database.

**Steps:**

1. **Ensure SQL Server is running** and accessible

2. **Update connection string based on your environment:**
   
   **For Development** (uses `appsettings.Development.json`):
   ```json
   // File: appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_COMPUTER\\SQLEXPRESS;Database=ForumAppDb;User Id=dev;Password=your_password;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Information"
       }
     }
   }
   ```
   
   **For Production** (uses `appsettings.json`):
   ```json
   // File: appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER\\INSTANCE;Database=ForumAppDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Ensure the SQL Server user has `dbcreator` role** (to create databases)

4. **Run the application** - The application will:
   - Automatically detect the environment and load the appropriate configuration file
   - EF Core will automatically:
     - Create the database if it doesn't exist
     - Apply all migrations
     - Seed the 3 forums via `HasData()` configuration

**Note:** The `dbcreator` role is only needed for the initial database creation. After that, the user needs appropriate permissions to access the database.

### Option 3: Using Manual SQL Script

If you prefer to create the database manually using SQL scripts, you can use the provided migration script.

**Steps:**

1. **Open SQL Server Management Studio (SSMS)** or your preferred SQL client

2. **Execute the migration script**:
   - Script location: `ForumApp.DAL/DBMigrations/V1_0_1__InitMigration.sql`
   - This script will:
     - Create the database
     - Create all tables (ASP.NET Identity, Forums, Posts, Comments)
     - Insert seed data (3 forums)
     - Set up all relationships and indexes

3. **Grant database access to the user from connection string**:
   
   After creating the database from the script, you need to grant the user (specified in your connection string) access to the database:
   
   **Important:** Run these SQL commands in SQL Server Management Studio as a database administrator:
   
   ```sql
   -- If using SQL Server authentication (User Id and Password in connection string)
   -- First, ensure the login exists (if not already created)
   USE [master];
   GO
   IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'YOUR_USER')
   BEGIN
       CREATE LOGIN [YOUR_USER] WITH PASSWORD = 'YOUR_PASSWORD';
   END
   GO
   
   -- Grant access to the database
   USE [ForumAppDb];
   GO
   
   -- Create user mapping if it doesn't exist
   IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'YOUR_USER')
   BEGIN
       CREATE USER [YOUR_USER] FOR LOGIN [YOUR_USER];
   END
   GO
   
   -- Grant necessary permissions (db_owner role for full access)
   ALTER ROLE [db_owner] ADD MEMBER [YOUR_USER];
   GO
   ```
   
   **Replace `YOUR_USER` with the actual username from your connection string.**
   
   **Alternative:** If you prefer minimal permissions instead of `db_owner`:
   ```sql
   USE [ForumAppDb];
   GO
   CREATE USER [YOUR_USER] FOR LOGIN [YOUR_USER];
   GO
   ALTER ROLE [db_datareader] ADD MEMBER [YOUR_USER];
   ALTER ROLE [db_datawriter] ADD MEMBER [YOUR_USER];
   ALTER ROLE [db_ddladmin] ADD MEMBER [YOUR_USER];
   GO
   ```
   
   **For Windows Authentication (Trusted_Connection=True):**
   ```sql
   USE [ForumAppDb];
   GO
   CREATE USER [DOMAIN\Username] FROM LOGIN [DOMAIN\Username];
   GO
   ALTER ROLE [db_owner] ADD MEMBER [DOMAIN\Username];
   GO
   ```

4. **Update connection string** based on your environment to point to your database:
   
   **For Development** (`appsettings.Development.json`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ForumAppDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     }
   }
   ```
   
   **For Production** (`appsettings.json`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ForumAppDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;"
     }
   }
   ```

5. **Run the application** - The application will:
   - Automatically detect the environment and load the appropriate configuration file
   - EF Core will detect that the database schema already exists and mark migrations as applied
   - The application will start successfully

**Note:** 
- When using the manual script, make sure the database schema matches what EF Core expects. The script includes all necessary tables and seed data.
- **Important:** The user from your connection string must have database access granted as shown in step 3. Without proper permissions, the application will fail to connect to the database.

### Database Initialization Details

Regardless of which option you choose, the application will:

- **Check for existing database** on startup
- **Apply migrations** if database exists but is not up-to-date
- **Create database and apply migrations** if database doesn't exist (Options 1 and 2)
- **Seed initial data** (3 forums) via `HasData()` in `ForumDbContext.OnModelCreating`
- **Log migration status** to console for debugging

**What gets created:**
- Database: `ForumAppDb`
- ASP.NET Identity tables (Users, Roles, UserClaims, etc.)
- Application tables: `Forums`, `Posts`, `Comments`
- Seed data: 3 forums (General Discussion, Tech Talk, Off Topic)

**Check console output** for migration status and any errors during startup.

## API Endpoints

All endpoints are documented in Swagger UI (available at `/swagger` in development mode).

### Authentication
- `POST /api/Account/register` - Register a new user (anonymous)
- `GET /api/Account/confirm-email?userId={userId}&token={token}` - Confirm user email (anonymous)
- `POST /api/Account/login` - Login user (anonymous)
- `POST /api/Account/logout` - Logout user (requires authentication)

#### Email Verification Process

After registering a new user, you need to confirm the email address before you can log in. The registration endpoint returns a confirmation link in the response.

**Option 1: Using the confirmation link (Easiest)**
1. Call `POST /api/Account/register` with user details
2. Copy the `ConfirmationLink` from the response (e.g., `https://localhost:7106/api/Account/confirm-email?userId=...&token=...`)
3. Open the link in your browser or make a GET request to that URL
4. You should receive a success message: `{ "Message": "Email confirmed successfully" }`

**Option 2: Using query parameters**
1. Call `POST /api/Account/register` with user details
2. Extract the `userId` and `token` from the `ConfirmationLink` in the response
3. Make a GET request to: `/api/Account/confirm-email?userId={userId}&token={token}`
4. You should receive a success message: `{ "Message": "Email confirmed successfully" }`

**Example Registration Response:**
```json
{
  "Message": "User registered successfully. Please confirm your email.",
  "ConfirmationLink": "https://localhost:7106/api/Account/confirm-email?userId=abc123&token=xyz789"
}
```

**Note:** Email confirmation is required before you can log in. The login endpoint will return an error if the email is not confirmed.

### Forums
- `GET /api/Forums` - Get all forums (public)
- `GET /api/Forums/{id}` - Get forum by ID (public)

### Posts
- `GET /api/Posts/first?count=10` - Get first posts (cached, public)
- `GET /api/Posts/forum/{forumId}?skip=0&take=10` - Get posts by forum with pagination (public)
- `GET /api/Posts/{id}` - Get post by ID (public)
- `POST /api/Posts` - Create a new post (requires authentication)
- `PUT /api/Posts/{id}` - Update a post (requires authentication, owner only)

### Comments
- `GET /api/Comments/post/{postId}` - Get all comments for a post (public)
- `GET /api/Comments/{id}` - Get comment by ID (public)
- `POST /api/Comments` - Create a new comment (requires authentication)
- `DELETE /api/Comments/{id}` - Delete a comment (requires authentication, post owner only)

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- One of the following database options:
  - **LocalDB** (recommended for development) - Usually comes with Visual Studio
  - **SQL Server / SQL Server Express** - Running and accessible
  - **SQL Server user with `dbcreator` role** (only needed for Option 2 - EF Core auto-creation)

### Steps

1. **Choose and Configure Database** (see [Database Setup](#database-setup) section above)
   - **Option 1:** Use LocalDB - Update `appsettings.Development.json` for Development environment or `appsettings.json` for Production
   - **Option 2:** Use SQL Server - Update `appsettings.Development.json` for Development or `appsettings.json` for Production
   - **Option 3:** Use manual script - Run the SQL script first, then update connection string in the appropriate file
   
   **Important:** The application automatically chooses which file to use based on the environment:
   - **Development mode** (default): Uses `appsettings.Development.json` (overrides `appsettings.json`)
   - **Production mode**: Uses `appsettings.json` only
   
   Edit the appropriate file based on your target environment:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
   }
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Build the Solution**
   ```bash
   dotnet build
   ```

4. **Run the Application**
   ```bash
   dotnet run --project ForumApp
   ```

5. **Access Swagger UI**
   Navigate to `https://localhost:7106/swagger` (or check console for actual port)

### What Happens on Startup

1. Application detects the environment (Development or Production)
2. Application loads configuration:
   - Always loads `appsettings.json`
   - If environment is Development, also loads `appsettings.Development.json` (overrides base settings)
3. Application reads connection string from the appropriate configuration file
4. EF Core connects to SQL Server
5. Database is created if it doesn't exist
6. All migrations are applied automatically
7. Seed data (3 forums) is inserted
8. Application logs migration status
9. API becomes available at the configured port

## Caching

The application implements in-memory caching for the first posts endpoint:

- **Cache Key**: `FirstPosts_{count}` (dynamic based on count parameter)
- **Cache Duration**: 
  - Absolute expiration: 5 minutes
  - Sliding expiration: 2 minutes
- **Cache Invalidation**: Automatically cleared when posts are created or updated
- **Implementation**: Microsoft.Extensions.Caching.Memory (IMemoryCache)

## Code Architecture

### Separation of Concerns

- **Controllers** (`ForumApp/ApiControllers`) - Handle HTTP requests/responses only
- **Managers** (`ForumApp.BL/Managers`) - Business logic and caching
- **DbManagers** (`ForumApp.DAL/DbManagers`) - Data access operations
- **Data Entities** (`ForumApp.Data`) - Domain models
- **DTOs** (`ForumApp.Models`) - Data transfer objects for API
- **Utils** (`ForumApp/Utils`) - Utility classes (e.g., DtoMapper)

### Key Design Patterns

- **Repository Pattern** - Data access abstraction via DbManagers (IForumsDB, IPostsDB, ICommentsDB)
- **Dependency Injection** - All services registered in Program.cs
- **DTO Pattern** - Entity-to-DTO mapping via DtoMapper utility
- **Hard Delete** - Comments are physically removed from database when deleted (no soft delete)

### Data Flow

```
API Controller → Manager → DbManager → DbContext → Database
              ↓
            DTOs
```

1. Controller receives HTTP request
2. Validates and maps to/from DTOs using DtoMapper
3. Calls appropriate Manager method
4. Manager applies business logic (caching, validation)
5. Manager calls DbManager for data access
6. DbManager uses DbContext to query/update database

## Code Quality Standards

✅ **Clean Code Principles**
- Proper separation of API and business logic
- Comprehensive XML documentation comments
- Meaningful method and variable names
- Single responsibility principle

✅ **Error Handling**
- Proper exception handling in managers and controllers
- Error response DTOs for consistent API error format
- Detailed logging for debugging

✅ **Validation**
- Model validation via data annotations
- Server-side validation in controllers
- Business rule validation in managers

## Development Notes

- **Authentication**: Cookie-based authentication (7-day expiration with sliding expiration)
- **Email Confirmation**: Auto-confirmed for simplicity (can be extended later)
- **Database Migrations**: Managed via EF Core migrations in `ForumApp/Migrations/`
- **Configuration** (Environment-Based):
  - `appsettings.json` - Base configuration for all environments (always loaded)
  - `appsettings.Development.json` - Development-specific settings (automatically loaded when `ASPNETCORE_ENVIRONMENT=Development`)
  - The application automatically selects the configuration file based on the environment:
    - **Development**: Uses `appsettings.Development.json` which overrides `appsettings.json`
    - **Production/Other**: Uses `appsettings.json` only
  - Environment is set in `ForumApp/Properties/launchSettings.json` for Visual Studio/`dotnet run`
  - Can be overridden with environment variable `ASPNETCORE_ENVIRONMENT`
- **CORS**: Enabled for all origins (configure appropriately for production)

## Project Files

### Important Files

- `ForumApp/Program.cs` - Application startup and configuration
- `ForumApp/appsettings.json` - Connection string and logging configuration
- `ForumApp.DAL/ForumDbContext.cs` - EF Core DbContext with entity configuration
- `ForumApp.DAL/DBMigrations/V1_0_1__InitMigration.sql` - Manual database initialization script
- `ForumApp/Utils/DtoMapper.cs` - Entity-to-DTO mapping utilities

### Entity Models (ForumApp.Data)

- `Forum.cs` - Forum entity with Name, Description, timestamps
- `Post.cs` - Post entity with Title, Content, ForumId, CreatedByUserId, IsDeleted flag
- `Comment.cs` - Comment entity with Content, PostId, CreatedByUserId, IsDeleted flag

### DTOs (ForumApp.Models)

- `DTOs/Auth/` - LoginDto, RegisterDto
- `DTOs/Forum/` - ForumDto
- `DTOs/Post/` - PostDto, CreatePostDto
- `DTOs/Comment/` - CommentDto, CreateCommentDto
- `DTOs/Error/` - ErrorResponseDto

## Troubleshooting

### Database Not Created

1. Check connection string in `appsettings.json`
2. Verify SQL Server is running
3. Ensure user has `dbcreator` role
4. Check application logs for error messages
5. Verify EF Core migrations exist in `ForumApp/Migrations/`

### Connection Errors

- Verify SQL Server instance name is correct
- Check user credentials (username/password)
- Ensure SQL Server allows remote connections
- Check firewall settings
- **If database was created from script:** Ensure the user from connection string has been granted access to the database:
  - Run: `CREATE USER [username] FOR LOGIN [username];`
  - Run: `ALTER ROLE [db_owner] ADD MEMBER [username];` (or grant `db_datareader`, `db_datawriter`, `db_ddladmin` roles)
  - See "Option 3: Using Manual SQL Script" section for detailed instructions

### Migration Errors

- Ensure `MigrationsAssembly` is set to `"ForumApp"` in Program.cs
- Check that migrations exist in `ForumApp/Migrations/` folder
- Verify connection string points to correct server

## License

This project is part of a forum application implementation following clean code standards and best practices.
