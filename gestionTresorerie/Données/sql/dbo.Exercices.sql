USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Exercices] Date du script : 06/04/2025 21:58:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Exercices];


GO
CREATE TABLE [dbo].[Exercices] (
    [Id]        INT  IDENTITY (1, 1) NOT NULL,
    [dateDebut] DATE NOT NULL,
    [dateFin]   DATE NOT NULL,
    [clos]      BIT  NOT NULL
);


