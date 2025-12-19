USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Exercices] Date du script : 18/12/2025 22:51:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Exercices] ;


GO
CREATE TABLE [dbo].[Exercices] (
    [Id]        INT  IDENTITY (1, 1) NOT NULL,
    [dateDebut] DATE NOT NULL,
    [dateFin]   DATE NOT NULL,
    [clos]      BIT  NOT NULL
);


