USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Mouvements] Date du script : 06/04/2025 21:59:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Mouvements];


GO
CREATE TABLE [dbo].[Mouvements] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [catégorie]        NVARCHAR (100) NOT NULL,
    [sousCatégorie]    NVARCHAR (100) NULL,
    [montant]          DECIMAL (7, 2) NOT NULL,
    [sens]             BIT            NOT NULL,
    [tiers]            NVARCHAR (50)  NULL,
    [note]             NVARCHAR (MAX) NULL,
    [dateMvt]          DATE           NOT NULL,
    [dateCréation]     DATE           NOT NULL,
    [dateModification] DATE           NULL,
    [etat]             BIT            NOT NULL,
    [événement]        NVARCHAR (50)  NULL,
    [type]             NVARCHAR (50)  NULL,
    [modifiable]       BIT            NOT NULL,
    [numeroRemise]     INT            NULL,
    [idCheque]         INT            NULL
);


