
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/12/2014 10:20:40
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
    ALTER TABLE [dbo].[Guesses] DROP CONSTRAINT [FK_GuessUser];
GO
IF OBJECT_ID(N'[dbo].[FK_NextGuesses]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Guesses] DROP CONSTRAINT [FK_NextGuesses];
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
IF OBJECT_ID(N'[dbo].[Words]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Words];
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
    [StartingWord] nvarchar(max)  NOT NULL,
    [GameOverMan] bit  NOT NULL,
    [Creator_Id] int  NULL
);
GO

-- Creating table 'Guesses'
CREATE TABLE [dbo].[Guesses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Order] int  NOT NULL,
    [Complete] bit  NOT NULL,
    [Game_Id] int  NOT NULL,
    [User_Id] int  NULL,
    [NextUser_Id] int  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [IsOnline] bit  NOT NULL
);
GO

-- Creating table 'Words'
CREATE TABLE [dbo].[Words] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [word] varchar(200)  NOT NULL
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

-- Creating primary key on [Id] in table 'Words'
ALTER TABLE [dbo].[Words]
ADD CONSTRAINT [PK_Words]
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

-- Creating foreign key on [Creator_Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [FK_UserGame]
    FOREIGN KEY ([Creator_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserGame'
CREATE INDEX [IX_FK_UserGame]
ON [dbo].[Games]
    ([Creator_Id]);
GO

-- Creating foreign key on [User_Id] in table 'Guesses'
ALTER TABLE [dbo].[Guesses]
ADD CONSTRAINT [FK_GuessUser]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GuessUser'
CREATE INDEX [IX_FK_GuessUser]
ON [dbo].[Guesses]
    ([User_Id]);
GO

-- Creating foreign key on [NextUser_Id] in table 'Guesses'
ALTER TABLE [dbo].[Guesses]
ADD CONSTRAINT [FK_NextGuesses]
    FOREIGN KEY ([NextUser_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_NextGuesses'
CREATE INDEX [IX_FK_NextGuesses]
ON [dbo].[Guesses]
    ([NextUser_Id]);
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