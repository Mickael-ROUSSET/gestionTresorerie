USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Coordonnees] Date du script : 18/12/2025 22:51:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Coordonnees];


GO
CREATE TABLE [dbo].[Coordonnees] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [IdTiers]     INT            NOT NULL,
    [TypeAdresse] NVARCHAR (50)  NULL,
    [Rue1]        NVARCHAR (100) NULL,
    [Rue2]        NVARCHAR (100) NULL,
    [CodePostal]  NVARCHAR (10)  NULL,
    [Ville]       NVARCHAR (50)  NULL,
    [Pays]        NVARCHAR (50)  NULL,
    [Email]       NVARCHAR (150) NULL,
    [Telephone]   NVARCHAR (30)  NULL
);


