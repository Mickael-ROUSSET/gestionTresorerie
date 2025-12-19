USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Categorie] Date du script : 18/12/2025 22:50:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Categorie];


GO
CREATE TABLE [dbo].[Categorie] (
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    [libelle]   VARCHAR (50) NOT NULL,
    [dateDebut] DATE         NOT NULL,
    [dateFin]   DATE         NULL,
    [debit]     INT          NOT NULL
);


