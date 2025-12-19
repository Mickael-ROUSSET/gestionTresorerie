USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Comptes] Date du script : 18/12/2025 22:51:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Comptes];


GO
CREATE TABLE [dbo].[Comptes] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [login]      NVARCHAR (50) NOT NULL,
    [motDePasse] NVARCHAR (50) NOT NULL,
    [typeAcces]  NCHAR (10)    NOT NULL
);


