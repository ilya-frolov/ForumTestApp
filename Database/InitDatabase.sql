-- Forum Application Database Initialization Script
-- This script creates the database and initializes it with 3 default forums
-- Run this script on SQL Server to set up the database

-- Note: The database schema will be created automatically by Entity Framework Core migrations
-- This script is provided for reference and manual database setup if needed

-- Create database (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ForumAppDb')
BEGIN
    CREATE DATABASE ForumAppDb;
END
GO

USE ForumAppDb;
GO

-- Note: The tables (Forums, Posts, Comments, AspNetUsers, etc.) will be created by EF Core
-- when you run the application or use migrations.

-- After running the application, you can verify the forums were created with:
-- SELECT * FROM Forums;

-- The application will automatically create these 3 forums on first run:
-- 1. General Discussion
-- 2. Technology  
-- 3. Help & Support

-- To manually insert forums (if needed), use:
/*
INSERT INTO Forums (Name, Description, CreatedAt)
VALUES 
    ('General Discussion', 'A place for general discussions and topics', GETUTCDATE()),
    ('Technology', 'Discuss the latest in technology, programming, and software', GETUTCDATE()),
    ('Help & Support', 'Get help and support from the community', GETUTCDATE());
*/

GO

