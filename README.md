# Forum Web Application

A simple forum web application built with ASP.NET Core (C#), using Entity Framework Core for data access and SQL Server as the database.

## Architecture

The project follows a clean architecture pattern with separation of concerns:

- **ForumApp**: Main web API project (controllers, configuration)
- **ForumApp.BL**: Business logic layer (managers, caching)
- **ForumApp.DAL**: Data access layer (repositories, DbContext)
- **ForumApp.Models**: Data models and DTOs

## Features

- User registration and authentication using ASP.NET Identity
- Three pre-configured forums:
  - General Discussion
  - Technology
  - Help & Support
- Users can create posts in forums
- Users can comment on posts
- Post creators can delete comments on their posts
- Caching mechanism for fetching first posts quickly

## Technologies

- **Server-side**: ASP.NET Core 8.0 (C#)
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: ASP.NET Identity with Cookie Authentication
- **Caching**: In-memory caching for first posts

## Database Setup

### Option 1: Automatic (Recommended)

The database will be created automatically when you run the application. The three forums will be initialized on first run.

### Option 2: Manual

1. Create a SQL Server database named `ForumAppDb`
2. Update the connection string in `appsettings.json` if needed
3. Run the application - EF Core will create the schema
4. Or use the provided `Database/InitDatabase.sql` script for reference

### Connection String

The default connection string uses LocalDB:
```
Server=(localdb)\mssqllocaldb;Database=ForumAppDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

For production, update the connection string in `appsettings.json` to point to your SQL Server instance.

## API Endpoints

### Authentication
- `POST /api/Account/register` - Register a new user
- `POST /api/Account/login` - Login user
- `POST /api/Account/logout` - Logout user (requires authentication)

### Forums
- `GET /api/Forums` - Get all forums (public)
- `GET /api/Forums/{id}` - Get forum by ID (public)

### Posts
- `GET /api/Posts/first?count=10` - Get first posts (cached, public)
- `GET /api/Posts/forum/{forumId}?skip=0&take=10` - Get posts by forum (public)
- `GET /api/Posts/{id}` - Get post by ID (public)
- `POST /api/Posts` - Create a new post (requires authentication)
- `PUT /api/Posts/{id}` - Update a post (requires authentication, owner only)
- `DELETE /api/Posts/{id}` - Delete a post (requires authentication, owner only)

### Comments
- `GET /api/Comments/post/{postId}` - Get comments for a post (public)
- `GET /api/Comments/{id}` - Get comment by ID (public)
- `POST /api/Comments` - Create a new comment (requires authentication)
- `DELETE /api/Comments/{id}` - Delete a comment (requires authentication, post owner only)

## Running the Application

1. Ensure SQL Server or LocalDB is installed
2. Update the connection string in `appsettings.json` if needed
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Build the solution:
   ```
   dotnet build
   ```
5. Run the application:
   ```
   dotnet run --project ForumApp
   ```
6. Navigate to `https://localhost:5001/swagger` to view the API documentation

## Caching

The application implements in-memory caching for the first posts endpoint (`/api/Posts/first`). The cache:
- Stores the first 10 posts by default
- Expires after 5 minutes (absolute expiration)
- Has a sliding expiration of 2 minutes
- Is automatically invalidated when posts are created, updated, or deleted

## Code Standards

- Clean code principles with proper separation of concerns
- Repository pattern for data access
- Business logic separated from API controllers
- Comprehensive XML comments for documentation
- Proper error handling and validation

## Development Notes

- The application uses cookie-based authentication
- Email confirmation is automatically handled (auto-confirmed for simplicity)
- All API endpoints return JSON responses
- Swagger UI is available in development mode for API testing

