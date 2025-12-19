USE [bddAgumaaaProd2]
GO

/****** Objet : Table [dbo].[Cheque] Date du script : 18/12/2025 22:50:47 ******/
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
    [banque]       NVARCHAR (50)   NOT NULL,
    [destinataire] NVARCHAR (50)   NULL,
    [imageChq]     VARBINARY (MAX) NULL
);


