USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Communes] Date du script : 18/12/2025 22:51:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Communes];


GO
CREATE TABLE [dbo].[Communes] (
    [Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [CodeINSEE]           VARCHAR (10)  NOT NULL,
    [NomCommune]          VARCHAR (150) NOT NULL,
    [CodePostal]          VARCHAR (10)  NOT NULL,
    [LibelleAcheminement] VARCHAR (150) NULL
);


