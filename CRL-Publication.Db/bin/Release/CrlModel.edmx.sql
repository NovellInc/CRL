
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/15/2015 10:23:04
-- Generated from EDMX file: C:\Users\grasman.mikhail\Documents\Visual Studio 2013\Projects\CRL-Publication\CRL-Publication.Db\CrlModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
Go
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CrlEntityMessageTask]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MessageTasks] DROP CONSTRAINT [FK_CrlEntityMessageTask];
GO
IF OBJECT_ID(N'[dbo].[FK_CrlEntityEventLog]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EventLogs] DROP CONSTRAINT [FK_CrlEntityEventLog];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CrlEntities]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CrlEntities];
GO
IF OBJECT_ID(N'[dbo].[MessageTasks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MessageTasks];
GO
IF OBJECT_ID(N'[dbo].[EventLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EventLogs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CrlEntities'
CREATE TABLE [dbo].[CrlEntities] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [NextUpdate] datetime  NOT NULL,
    [File] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'MessageTasks'
CREATE TABLE [dbo].[MessageTasks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SendingTime] datetime  NOT NULL,
    [CrlEntity_Id] int  NOT NULL
);
GO

-- Creating table 'EventLogs'
CREATE TABLE [dbo].[EventLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EventDate] datetime  NOT NULL,
    [EventResult] int  NOT NULL,
    [Message] nvarchar(max)  NOT NULL,
    [CrlEntityId] int  NOT NULL,
    [PreviousCrlFile] nvarchar(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'CrlEntities'
ALTER TABLE [dbo].[CrlEntities]
ADD CONSTRAINT [PK_CrlEntities]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MessageTasks'
ALTER TABLE [dbo].[MessageTasks]
ADD CONSTRAINT [PK_MessageTasks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EventLogs'
ALTER TABLE [dbo].[EventLogs]
ADD CONSTRAINT [PK_EventLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CrlEntity_Id] in table 'MessageTasks'
ALTER TABLE [dbo].[MessageTasks]
ADD CONSTRAINT [FK_CrlEntityMessageTask]
    FOREIGN KEY ([CrlEntity_Id])
    REFERENCES [dbo].[CrlEntities]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CrlEntityMessageTask'
CREATE INDEX [IX_FK_CrlEntityMessageTask]
ON [dbo].[MessageTasks]
    ([CrlEntity_Id]);
GO

-- Creating foreign key on [CrlEntityId] in table 'EventLogs'
ALTER TABLE [dbo].[EventLogs]
ADD CONSTRAINT [FK_CrlEntityEventLog]
    FOREIGN KEY ([CrlEntityId])
    REFERENCES [dbo].[CrlEntities]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CrlEntityEventLog'
CREATE INDEX [IX_FK_CrlEntityEventLog]
ON [dbo].[EventLogs]
    ([CrlEntityId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------