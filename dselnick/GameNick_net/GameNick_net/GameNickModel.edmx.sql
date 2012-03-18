
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/23/2011 22:12:05
-- Generated from EDMX file: C:\Users\Daniel\Desktop\dselnick\GameNick.net\GameNick.net\GameNickModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [gamenick];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Events_Games]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Events] DROP CONSTRAINT [FK_Events_Games];
GO
IF OBJECT_ID(N'[dbo].[FK_GameNicks_Games]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameNicks] DROP CONSTRAINT [FK_GameNicks_Games];
GO
IF OBJECT_ID(N'[dbo].[FK_GameNicks_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameNicks] DROP CONSTRAINT [FK_GameNicks_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_Games_Platforms]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Games] DROP CONSTRAINT [FK_Games_Platforms];
GO
IF OBJECT_ID(N'[dbo].[FK_Games_Services]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Games] DROP CONSTRAINT [FK_Games_Services];
GO
IF OBJECT_ID(N'[dbo].[FK_Services_Platforms]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Services] DROP CONSTRAINT [FK_Services_Platforms];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Events]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Events];
GO
IF OBJECT_ID(N'[dbo].[GameNicks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameNicks];
GO
IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Games];
GO
IF OBJECT_ID(N'[dbo].[Platforms]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Platforms];
GO
IF OBJECT_ID(N'[dbo].[Services]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Services];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Events'
CREATE TABLE [dbo].[Events] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [GameID] int  NOT NULL,
    [FacebookID] int  NOT NULL
);
GO

-- Creating table 'GameNicks'
CREATE TABLE [dbo].[GameNicks] (
    [UserID] int  NOT NULL,
    [GameID] int  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [ID] int IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [PlatformID] int  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [ServiceID] int  NOT NULL
);
GO

-- Creating table 'Platforms'
CREATE TABLE [dbo].[Platforms] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Services'
CREATE TABLE [dbo].[Services] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [PlatformID] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [facebookid] bigint  NOT NULL,
    [accesstoken] varchar(1000)  NOT NULL,
    [id] int IDENTITY(1,1) NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [PK_Events]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'GameNicks'
ALTER TABLE [dbo].[GameNicks]
ADD CONSTRAINT [PK_GameNicks]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Platforms'
ALTER TABLE [dbo].[Platforms]
ADD CONSTRAINT [PK_Platforms]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Services'
ALTER TABLE [dbo].[Services]
ADD CONSTRAINT [PK_Services]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [GameID] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [FK_Events_Games]
    FOREIGN KEY ([GameID])
    REFERENCES [dbo].[Games]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Events_Games'
CREATE INDEX [IX_FK_Events_Games]
ON [dbo].[Events]
    ([GameID]);
GO

-- Creating foreign key on [GameID] in table 'GameNicks'
ALTER TABLE [dbo].[GameNicks]
ADD CONSTRAINT [FK_GameNicks_Games]
    FOREIGN KEY ([GameID])
    REFERENCES [dbo].[Games]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameNicks_Games'
CREATE INDEX [IX_FK_GameNicks_Games]
ON [dbo].[GameNicks]
    ([GameID]);
GO

-- Creating foreign key on [UserID] in table 'GameNicks'
ALTER TABLE [dbo].[GameNicks]
ADD CONSTRAINT [FK_GameNicks_Users]
    FOREIGN KEY ([UserID])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameNicks_Users'
CREATE INDEX [IX_FK_GameNicks_Users]
ON [dbo].[GameNicks]
    ([UserID]);
GO

-- Creating foreign key on [PlatformID] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [FK_Games_Platforms]
    FOREIGN KEY ([PlatformID])
    REFERENCES [dbo].[Platforms]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Games_Platforms'
CREATE INDEX [IX_FK_Games_Platforms]
ON [dbo].[Games]
    ([PlatformID]);
GO

-- Creating foreign key on [ID] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [FK_Games_Services]
    FOREIGN KEY ([ID])
    REFERENCES [dbo].[Services]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PlatformID] in table 'Services'
ALTER TABLE [dbo].[Services]
ADD CONSTRAINT [FK_Services_Platforms]
    FOREIGN KEY ([PlatformID])
    REFERENCES [dbo].[Platforms]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Services_Platforms'
CREATE INDEX [IX_FK_Services_Platforms]
ON [dbo].[Services]
    ([PlatformID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------