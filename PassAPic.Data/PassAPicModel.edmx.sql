
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/08/2014 18:48:28
-- Generated from EDMX file: c:\Users\graha_000\documents\visual studio 2013\Projects\PassAPic\PassAPic.Data\PassAPicModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [master];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [NumberOfParticipants] int  NOT NULL,
    [GeneratedWord] nvarchar(max)  NOT NULL,
    [Users_Id] int  NULL
);
GO

-- Creating table 'Turns'
CREATE TABLE [dbo].[Turns] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Word] nvarchar(max)  NOT NULL,
    [Image] nvarchar(max)  NOT NULL,
    [Game_Id] int  NOT NULL,
    [Users_Id] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DeviceGuid] uniqueidentifier  NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [IsOnline] bit  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Turns'
ALTER TABLE [dbo].[Turns]
ADD CONSTRAINT [PK_Turns]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Game_Id] in table 'Turns'
ALTER TABLE [dbo].[Turns]
ADD CONSTRAINT [FK_GameTurn]
    FOREIGN KEY ([Game_Id])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameTurn'
CREATE INDEX [IX_FK_GameTurn]
ON [dbo].[Turns]
    ([Game_Id]);
GO

-- Creating foreign key on [Users_Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [FK_UserGame]
    FOREIGN KEY ([Users_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserGame'
CREATE INDEX [IX_FK_UserGame]
ON [dbo].[Games]
    ([Users_Id]);
GO

-- Creating foreign key on [Users_Id] in table 'Turns'
ALTER TABLE [dbo].[Turns]
ADD CONSTRAINT [FK_TurnUser]
    FOREIGN KEY ([Users_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TurnUser'
CREATE INDEX [IX_FK_TurnUser]
ON [dbo].[Turns]
    ([Users_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------