USE [bddAgumaaa]
GO

/****** Objet : Table [dbo].[Mouvements] Date du script : 18/12/2025 22:51:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[Mouvements];


GO
CREATE TABLE [dbo].[Mouvements] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [categorie]        INT            NOT NULL,
    [sousCategorie]    INT            NULL,
    [montant]          DECIMAL (7, 2) NOT NULL,
    [sens]             BIT            NOT NULL,
    [tiers]            INT            NOT NULL,
    [note]             NVARCHAR (500) NULL,
    [dateMvt]          DATE           NOT NULL,
    [dateCreation]     DATE           NOT NULL,
    [dateModification] DATE           NULL,
    [etat]             BIT            NOT NULL,
    [evenement]        NVARCHAR (50)  NULL,
    [typeMouvement]    NVARCHAR (50)  NULL,
    [modifiable]       BIT            NOT NULL,
    [numeroRemise]     INT            NULL,
    [idCheque]         INT            NULL,
    [reference]        NVARCHAR (50)  NULL,
    [typeReference]    NVARCHAR (50)  NULL,
    [idDoc]            INT            NULL
);


