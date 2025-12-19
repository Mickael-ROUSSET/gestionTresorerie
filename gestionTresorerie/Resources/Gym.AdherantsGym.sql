USE [bddAgumaaa]
GO

/****** Objet : Table [Gym].[AdherantsGym] Date du script : 18/12/2025 22:52:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [Gym].[AdherantsGym];


GO
CREATE TABLE [Gym].[AdherantsGym] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [IdTiers]              INT            NOT NULL,
    [IdAdresse]            INT            NULL,
    [Cours]                NVARCHAR (100) NULL,
    [Questionnaire]        BIT            NOT NULL,
    [PaiementEspece]       BIT            NOT NULL,
    [NumCheque]            NVARCHAR (30)  NULL,
    [FormulaireAdhesion]   BIT            NOT NULL,
    [NumLicence]           INT            NULL,
    [DateMiseAJourLicence] DATETIME2 (7)  NULL
);


