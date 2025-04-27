USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[corrMvtDoc] Date du script : 06/04/2025 21:58:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[corrMvtDoc];


GO
CREATE TABLE [dbo].[corrMvtDoc] (
    [Id]    INT NOT NULL,
    [idMvt] INT NOT NULL,
    [idDoc] INT NOT NULL
);


