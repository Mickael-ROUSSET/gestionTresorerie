USE [bddAgumaaaProd2]
GO

/****** Objet : Table [dbo].[TypeDoc] Date du script : 18/12/2025 22:52:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[TypeDoc];


GO
CREATE TABLE [dbo].[TypeDoc] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [prompt]            NVARCHAR (MAX) NULL,
    [libellé]           NVARCHAR (50)  NOT NULL,
    [gabaritRepertoire] NVARCHAR (MAX) NULL,
    [gabaritNomFichier] NVARCHAR (MAX) NULL,
    [classe]            NVARCHAR (50)  NULL
);


