USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Documents] Date du script : 06/04/2025 21:58:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Documents];


GO
CREATE TABLE [dbo].[Documents] (
    [idDoc]            INT            IDENTITY (1, 1) NOT NULL,
    [dateDoc]          DATE           NOT NULL,
    [contenuDoc]       NVARCHAR (MAX) NOT NULL,
    [cheminDoc]        NVARCHAR (MAX) NOT NULL,
    [categorieDoc]     NVARCHAR (50)  NULL,
    [sousCategorieDoc] NVARCHAR (50)  NULL,
    [idMvtDoc]         INT            NULL
);


