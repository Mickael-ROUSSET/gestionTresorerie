USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[SousCategorie] Date du script : 18/12/2025 22:51:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[SousCategorie];


GO
CREATE TABLE [dbo].[SousCategorie] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [libelle]     VARCHAR (50) NOT NULL,
    [dateDebut]   DATE         NOT NULL,
    [dateFin]     DATE         NULL,
    [idCategorie] INT          NOT NULL
);


