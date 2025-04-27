USE [G:\MON DRIVE\AGUMAAA\DOCUMENTS\BACASABLE\BDDAGUMAAA.MDF]
GO

/****** Objet : Table [dbo].[Cheque] Date du script : 06/04/2025 21:58:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[Cheque];


GO
CREATE TABLE [dbo].[Cheque] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [numero]       INT             NULL,
    [date]         DATE            NULL,
    [emetteur]     NVARCHAR (50)   NOT NULL,
    [montant]      DECIMAL (10, 2) NULL,
    [banque]       NVARCHAR (50)   DEFAULT ('A déterminer') NOT NULL,
    [destinataire] NVARCHAR (50)   DEFAULT ('A déterminer') NULL,
    [imageChq]     VARBINARY (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
