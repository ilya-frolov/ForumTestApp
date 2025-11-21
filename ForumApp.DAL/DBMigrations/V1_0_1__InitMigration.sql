-- Migration: V1_0_1__InitMigration.sql
-- Description: Initial database schema creation for Forum Application
-- Includes: ASP.NET Identity tables, Forums, Posts, Comments tables, and seed data

-- ============================================
-- ASP.NET Identity Tables
-- ============================================

-- Create the database
CREATE DATABASE ForumAppDb;
GO
USE ForumAppDb;
GO

-- ============================================
-- ASP.NET Identity Tables
-- ============================================

-- AspNetRoles table
CREATE TABLE [dbo].[AspNetRoles] (
    [Id] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(256) NULL,
    [NormalizedName] NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- AspNetRoleClaims table
CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) 
        REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUsers table
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] NVARCHAR(450) NOT NULL,
    [UserName] NVARCHAR(256) NULL,
    [NormalizedUserName] NVARCHAR(256) NULL,
    [Email] NVARCHAR(256) NULL,
    [NormalizedEmail] NVARCHAR(256) NULL,
    [EmailConfirmed] BIT NOT NULL DEFAULT 0,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL DEFAULT 0,
    [TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
    [LockoutEnd] DATETIMEOFFSET NULL,
    [LockoutEnabled] BIT NOT NULL DEFAULT 0,
    [AccessFailedCount] INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- AspNetUserClaims table
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserLogins table
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [ProviderKey] NVARCHAR(450) NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX) NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserRoles table
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR(450) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) 
        REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- AspNetUserTokens table
CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId] NVARCHAR(450) NOT NULL,
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(450) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- ============================================
-- Forum Application Tables
-- ============================================

-- Forums table
CREATE TABLE [dbo].[Forums] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [PK_Forums] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Posts table
CREATE TABLE [dbo].[Posts] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(500) NOT NULL,
    [Content] NVARCHAR(4000) NOT NULL,
    [ForumId] INT NOT NULL,
    [CreatedByUserId] NVARCHAR(450) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Posts_Forums_ForumId] FOREIGN KEY ([ForumId]) 
        REFERENCES [dbo].[Forums] ([Id]) ON DELETE NO ACTION
);
GO

-- Comments table
CREATE TABLE [dbo].[Comments] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Content] NVARCHAR(1000) NOT NULL,
    [PostId] INT NOT NULL,
    [CreatedByUserId] NVARCHAR(450) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY ([PostId]) 
        REFERENCES [dbo].[Posts] ([Id]) ON DELETE CASCADE
);
GO

-- ============================================
-- Indexes
-- ============================================

-- Identity indexes
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) 
    WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) 
    WHERE [NormalizedUserName] IS NOT NULL;
GO

-- Forum application indexes
CREATE UNIQUE NONCLUSTERED INDEX [IX_Forums_Name] ON [dbo].[Forums] ([Name]);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_ForumId] ON [dbo].[Posts] ([ForumId]);
GO

CREATE NONCLUSTERED INDEX [IX_Posts_CreatedAt] ON [dbo].[Posts] ([CreatedAt]);
GO

CREATE NONCLUSTERED INDEX [IX_Comments_PostId] ON [dbo].[Comments] ([PostId]);
GO

CREATE NONCLUSTERED INDEX [IX_Comments_CreatedAt] ON [dbo].[Comments] ([CreatedAt]);
GO

-- ============================================
-- Seed Data
-- ============================================

-- Insert initial forums (enable identity insert to specify Id values)
SET IDENTITY_INSERT [dbo].[Forums] ON;

INSERT INTO [dbo].[Forums] ([Id], [Name], [Description], [CreatedAt])
VALUES 
    (1, N'General Discussion', N'Talk about anything', GETUTCDATE()),
    (2, N'Tech Talk', N'Discuss technology and programming', GETUTCDATE()),
    (3, N'Off Topic', N'Casual conversations and fun', GETUTCDATE());
GO

SET IDENTITY_INSERT [dbo].[Forums] OFF;
GO
