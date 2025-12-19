USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Tiers] Date du script : 18/12/2025 22:51:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Tiers];


GO
CREATE TABLE [dbo].[Tiers] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [nom]                 NVARCHAR (50)  NULL,
    [prenom]              NVARCHAR (50)  NULL,
    [raisonSociale]       NVARCHAR (50)  NULL,
    [categorieDefaut]     INT            NULL,
    [sousCategorieDefaut] INT            NULL,
    [dateCreation]        DATETIME       NOT NULL,
    [dateModification]    DATETIME       NOT NULL,
    [dateNaissance]       DATE           NULL,
    [lieuNaissance]       NVARCHAR (100) NULL
);


