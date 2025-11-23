USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Tiers] Date du script : 06/04/2025 21:59:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Tiers];


GO
CREATE TABLE [dbo].[Tiers] (
    [Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [nom]                 NVARCHAR (50) NULL,
    [prenom]              NVARCHAR (50) NULL,
    [raisonSociale]       NVARCHAR (50) NULL,
    [categorieDefaut]     INT           NULL,
    [sousCategorieDefaut] INT           NULL,
    [dateCreation]        DATETIME      NOT NULL,
    [dateModification]    DATETIME      NOT NULL
);


