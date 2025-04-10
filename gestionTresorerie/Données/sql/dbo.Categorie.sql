USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Categorie] Date du script : 06/04/2025 21:58:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Categorie];


GO
CREATE TABLE [dbo].[Categorie] (
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    [libelle]   VARCHAR (50) NOT NULL,
    [dateDebut] DATE         NOT NULL,
    [dateFin]   DATE         NULL,
    [debit]     INT          NOT NULL
);


