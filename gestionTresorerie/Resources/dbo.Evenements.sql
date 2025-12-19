USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Evenement] Date du script : 18/12/2025 22:51:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Evenements];


GO
CREATE TABLE [dbo].[Evenements] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [libelle]   NVARCHAR (MAX) NULL,
    [dateDebut] DATE           NULL,
    [dateFin]   DATE           NULL
);


