USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[SousCategorie] Date du script : 06/04/2025 21:59:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[SousCategorie];


GO
CREATE TABLE [dbo].[SousCategorie] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [libelle]     VARCHAR (50) NOT NULL,
    [dateDebut]   DATE         NOT NULL,
    [dateFin]     DATE         NULL,
    [idCategorie] INT          NOT NULL
);


