USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Type] Date du script : 18/12/2025 22:52:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Type];


GO
CREATE TABLE [dbo].[Type] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [libelle]   NVARCHAR (MAX) NULL,
    [dateDebut] DATE           NULL,
    [dateFin]   DATE           NULL
);


