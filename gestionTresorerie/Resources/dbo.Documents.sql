USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Documents] Date du script : 18/12/2025 22:51:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Documents];


GO
CREATE TABLE [dbo].[Documents] (
    [idDoc]            INT            IDENTITY (1, 1) NOT NULL,
    [dateDoc]          DATETIME       NOT NULL,
    [contenuDoc]       NVARCHAR (MAX) NULL,
    [cheminDoc]        NVARCHAR (MAX) NOT NULL,
    [categorieDoc]     NVARCHAR (50)  NULL,
    [sousCategorieDoc] NVARCHAR (50)  NULL,
    [idMvtDoc]         INT            NULL,
    [metaDonnees]      NVARCHAR (MAX) NULL,
    [dateModif]        DATETIME       NULL
);


