USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Evenements] Date du script : 06/04/2025 21:58:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Evenements];


GO
CREATE TABLE [dbo].[Evenements] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [libelle] NVARCHAR (50) NOT NULL,
    [periode] BIGINT        NOT NULL
);


