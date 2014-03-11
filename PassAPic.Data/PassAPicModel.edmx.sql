
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/11/2014 10:05:37
-- Generated from EDMX file: C:\YerMA\PassAPic\PassAPic.Data\PassAPicModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [PassAPic];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_GameTurn]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Guesses] DROP CONSTRAINT [FK_GameTurn];
GO
IF OBJECT_ID(N'[dbo].[FK_UserGame]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Games] DROP CONSTRAINT [FK_UserGame];
GO
IF OBJECT_ID(N'[dbo].[FK_GuessUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_GuessUser];
GO
IF OBJECT_ID(N'[dbo].[FK_WordGuess_inherits_Guess]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Guesses_WordGuess] DROP CONSTRAINT [FK_WordGuess_inherits_Guess];
GO
IF OBJECT_ID(N'[dbo].[FK_ImageGuess_inherits_Guess]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Guesses_ImageGuess] DROP CONSTRAINT [FK_ImageGuess_inherits_Guess];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Games];
GO
IF OBJECT_ID(N'[dbo].[Guesses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Guesses];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Guesses_WordGuess]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Guesses_WordGuess];
GO
IF OBJECT_ID(N'[dbo].[Guesses_ImageGuess]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Guesses_ImageGuess];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [NumberOfGuesses] int  NOT NULL,
    [GeneratedWord] nvarchar(max)  NOT NULL,
    [Users_Id] int  NULL
);
GO

-- Creating table 'Guesses'
CREATE TABLE [dbo].[Guesses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Order] int  NOT NULL,
    [Game_Id] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [IsOnline] bit  NOT NULL,
    [Guess_Id] int  NULL
);
GO

-- Creating table 'Guesses_WordGuess'
CREATE TABLE [dbo].[Guesses_WordGuess] (
    [Word] nvarchar(max)  NOT NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'Guesses_ImageGuess'
CREATE TABLE [dbo].[Guesses_ImageGuess] (
    [Image] nvarchar(max)  NOT NULL,
    [Id] int  NOT NULL
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

-- Creating primary key on [Id] in table 'Guesses'
ALTER TABLE [dbo].[Guesses]
ADD CONSTRAINT [PK_Guesses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Guesses_WordGuess'
ALTER TABLE [dbo].[Guesses_WordGuess]
ADD CONSTRAINT [PK_Guesses_WordGuess]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Guesses_ImageGuess'
ALTER TABLE [dbo].[Guesses_ImageGuess]
ADD CONSTRAINT [PK_Guesses_ImageGuess]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Game_Id] in table 'Guesses'
ALTER TABLE [dbo].[Guesses]
ADD CONSTRAINT [FK_GameTurn]
    FOREIGN KEY ([Game_Id])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameTurn'
CREATE INDEX [IX_FK_GameTurn]
ON [dbo].[Guesses]
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

-- Creating foreign key on [Guess_Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_GuessUser]
    FOREIGN KEY ([Guess_Id])
    REFERENCES [dbo].[Guesses]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GuessUser'
CREATE INDEX [IX_FK_GuessUser]
ON [dbo].[Users]
    ([Guess_Id]);
GO

-- Creating foreign key on [Id] in table 'Guesses_WordGuess'
ALTER TABLE [dbo].[Guesses_WordGuess]
ADD CONSTRAINT [FK_WordGuess_inherits_Guess]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Guesses]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Guesses_ImageGuess'
ALTER TABLE [dbo].[Guesses_ImageGuess]
ADD CONSTRAINT [FK_ImageGuess_inherits_Guess]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Guesses]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------