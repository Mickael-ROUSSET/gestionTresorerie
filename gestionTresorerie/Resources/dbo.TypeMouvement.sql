USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[TypeMouvement] Date du script : 18/12/2025 22:52:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[TypeMouvement];


GO
CREATE TABLE [dbo].[TypeMouvement] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [libelle]   NVARCHAR (MAX) NULL,
    [dateDebut] DATE           NULL,
    [dateFin]   DATE           NULL
);


