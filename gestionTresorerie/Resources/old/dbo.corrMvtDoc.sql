USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[corrMvtDoc] Date du script : 18/12/2025 22:51:20 ******/
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


